using VoicerStudio.Application.Models;

namespace VoicerStudio.Application.Services;

public interface ICognitiveService
{
    Task<Language[]> GetLanguagesAsync(string credentials);
    Task<SpeechGenerateResult> GenerateSpeechAsync(SpeechGenerateRequest request, string credentials);
    Task<SpeechGenerateResult> GenerateSpeechAsync(SpeechGenerateRequest[] requests, string credentials);
}