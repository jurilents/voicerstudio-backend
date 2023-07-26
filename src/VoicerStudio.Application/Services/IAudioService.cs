using VoicerStudio.Application.Enums;

namespace VoicerStudio.Application.Services;

public record ResizeResult(
    Stream Audio,
    TimeSpan InputDuration,
    TimeSpan OutputDuration,
    bool Success = true
);

public interface IAudioService
{
    AudioFormat AudioFormat { get; }
    Task<Stream> MergeAsync(Stream[] audioFiles, double[] offsetsInMilliseconds);
    Task<ResizeResult> ResizeAsync(Stream audioFile, TimeSpan targetDuration);
    TimeSpan GetAudioDuration(Stream stream);
}