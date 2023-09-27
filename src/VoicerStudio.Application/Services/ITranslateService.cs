using VoicerStudio.Application.Models;

namespace VoicerStudio.Application.Services;

public interface ITranslateService
{
    Task<TranslationLanguages> GetLanguagesAsync(CancellationToken ct = default);
    Task<TranslationModel[]> TranslateAsync(TranslateRequest request, CancellationToken ct = default);
}