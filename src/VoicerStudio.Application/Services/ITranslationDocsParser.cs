using VoicerStudio.Application.Models.TranslationDocs;

namespace VoicerStudio.Application.Services;

public interface ITranslationDocsParser
{
    Task<ParseTranslationDocResult> ParseFileAsync(string fileId, ParseTranslationDocQuery query, CancellationToken ct = default);
}