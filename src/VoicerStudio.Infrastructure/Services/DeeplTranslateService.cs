using DeepL;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Infrastructure.Services;

public class DeeplTranslateService : ITranslateService
{
    private readonly DeeplOptions _options;
    private readonly IMemoryCache _memoryCache;

    public DeeplTranslateService(IOptions<DeeplOptions> optionsAccessor, IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _options = optionsAccessor.Value;
    }


    public Task<TranslationLanguages> GetLanguagesAsync(CancellationToken ct = default)
    {
        return _memoryCache.GetOrCreateAsync("DeeplLanguages", async _ =>
        {
            var translator = new Translator(_options.ApiKey);
            var source = translator.GetSourceLanguagesAsync(ct);
            var target = translator.GetTargetLanguagesAsync(ct);
            await Task.WhenAll(source, target);

            return new TranslationLanguages
            {
                SourceLanguages = source.Result.Select(x => new Language { Locale = x.Code, DisplayName = x.Name }).ToArray(),
                TargetLanguages = target.Result.Select(x => new Language { Locale = x.Code, DisplayName = x.Name }).ToArray(),
            };
        })!;
    }

    public async Task<TranslationModel[]> TranslateAsync(TranslateRequest request, CancellationToken ct = default)
    {
        var translator = new Translator(_options.ApiKey);
        var result = await translator.TranslateTextAsync(
            request.Texts, request.SourceLang, request.TargetLang, cancellationToken: ct);
        return result.Select((x, index) => new TranslationModel
        {
            Text = request.Texts[index],
            Translation = x.Text,
        }).ToArray();
    }
}