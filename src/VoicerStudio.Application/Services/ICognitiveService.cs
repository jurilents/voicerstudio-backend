using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models;

namespace VoicerStudio.Application.Services;

public interface ICognitiveService
{
    CognitiveServiceName ServiceName { get; }
    Task<Language[]> GetLanguagesAsync(string credentials);
    Task<SpeechGenerateResult> GenerateSpeechAsync(SpeechGenerateRequest request, string credentials);
    Task<SpeechGenerateResult> GenerateSpeechAsync(SpeechGenerateRequest[] requests, string credentials);
}