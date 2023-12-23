using VoicerStudio.Application.Models.Recognition;

namespace VoicerStudio.Application.Services;

public interface ISpeechRecognitionService
{
    Task<RecognitionResult> RecognizeAsync(Stream audioFile, CancellationToken ct = default);
}