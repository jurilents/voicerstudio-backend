using System.Globalization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoicerStudio.Application.Models.TranslationDocs;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.Infrastructure;

public class TranslationDocsParser : ITranslationDocsParser
{
    private static readonly string[] scopes = { DocsService.Scope.DriveReadonly };

    private readonly ILogger<TranslationDocsParser> _logger;
    private readonly GoogleOptions _options;

    private DocsService Service { get; }

    public TranslationDocsParser(ILogger<TranslationDocsParser> logger, IOptions<GoogleOptions> optionsAccessor)
    {
        _logger = logger;
        _logger = logger;
        _options = optionsAccessor.Value;
        var credential = GetCredentialsFromFile();

        Service = new DocsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName,
        });
    }


    public async Task<ParseTranslationDocResult> ParseFileAsync(
        string fileId, ParseTranslationDocQuery query, CancellationToken ct = default)
    {
        var result = new ParseTranslationDocResult();
        var targetCulture = new CultureInfo(query.Language);
        var targetLangCode = targetCulture.ThreeLetterISOLanguageName.ToLower();

        var baseCultures = query.BaseLanguages
            .Split(',')
            .Select(x => new CultureInfo(x))
            .Select(x => x.ThreeLetterISOLanguageName.ToLower())
            .ToArray();

        // Export the Google Doc as plain text
        var request = Service.Documents.Get(fileId);
        var document = await request.ExecuteAsync(ct);

        var tableNodes = document.Body.Content.Where(x => x.Table is not null);

        foreach (var tableElement in tableNodes)
        {
            // Process table headers
            int baseIndex = -1, langIndex = -1;
            var headerCells = tableElement.Table.TableRows[0].TableCells;
            for (var i = 0; i < headerCells.Count; i++)
            {
                var headerElement = headerCells[i];
                var langName = ReadTextContent(headerElement.Content);
                if (string.IsNullOrEmpty(langName)) langName = query.Language;
                var cultureInfo = new CultureInfo(langName);
                var langCode = cultureInfo.ThreeLetterISOLanguageName.ToLower();

                if (langCode == targetLangCode)
                {
                    // result.Lang = new LanguageModel
                    // {
                    //     Code = langCode,
                    //     Name = cultureInfo.EnglishName,
                    // };
                    langIndex = i;
                }
                else if (baseCultures.Any(x => x == langCode))
                {
                    baseIndex = i;
                }
            }

            // Process table cells
            foreach (var rowElement in tableElement.Table.TableRows.Skip(1))
            {
                var baseCell = rowElement.TableCells[baseIndex];
                var baseCellText = ReadTextContent(baseCell.Content);

                var langCell = rowElement.TableCells[langIndex];
                var langCellText = ReadTextContent(langCell.Content);

                if (query.SplitByParagraph)
                {
                    using var baseCellParagraphs = baseCell.Content.SelectMany(ReadTextContentSplit).GetEnumerator();
                    using var langCellParagraphs = langCell.Content.SelectMany(ReadTextContentSplit).GetEnumerator();

                    while (true)
                    {
                        var baseCompleted = !baseCellParagraphs.MoveNext();
                        var langCompleted = !langCellParagraphs.MoveNext();
                        if (baseCompleted && langCompleted) break;

                        var translation = new TextTranslationModel
                        {
                            Original = baseCompleted ? "" : baseCellParagraphs.Current,
                            Translation = langCompleted ? "" : langCellParagraphs.Current,
                            // SpeakerName = null,
                        };

                        if (translation.IsValid())
                        {
                            translation.Normalize();
                            result.Translations.Add(translation);
                        }
                    }
                }
                else
                {
                    var translation = new TextTranslationModel
                    {
                        Original = baseCellText,
                        Translation = langCellText,
                        // SpeakerName = null,
                    };

                    if (translation.IsValid())
                    {
                        translation.Normalize();
                        result.Translations.Add(translation);
                    }
                }
            }
        }


        return result;
    }

    private string ReadTextContent(StructuralElement element)
    {
        if (element.Paragraph is null) return "";
        var textParts = element.Paragraph.Elements
            .Select(x => x.TextRun.Content.Trim('\r', '\n'));
        return string.Join("", textParts);
    }

    private string ReadTextContent(IEnumerable<StructuralElement> elements)
    {
        return string.Join("", elements.Select(ReadTextContent));
    }

    private IEnumerable<string> ReadTextContentSplit(StructuralElement element)
    {
        if (element.Paragraph is null) return ArraySegment<string>.Empty;
        var textParts = element.Paragraph.Elements
            .SelectMany(x => x.TextRun.Content.Split('\v')) // vertical tab sign
            .Select(x => x.Trim('\n').Trim('\r'))
            .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        return textParts;
    }


    private GoogleCredential GetCredentialsFromFile()
    {
        using var stream = new FileStream(_options.SecretsPath, FileMode.Open, FileAccess.Read);
        var credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
        return credential;
    }
}