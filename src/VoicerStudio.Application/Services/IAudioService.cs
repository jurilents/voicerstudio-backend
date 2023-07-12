using VoicerStudio.Application.Infrastructure;

namespace VoicerStudio.Application.Services;

public record ResizeResult(
    bool Success,
    byte[] AudioData,
    TimeSpan InputDuration,
    TimeSpan OutputDuration
);

public interface IAudioService
{
    Task<byte[]> MergeAsync(byte[][] audioFiles, double[] offsetsInMilliseconds);
    Task<ResizeResult> ResizeAsync(byte[] audioFile, TimeSpan targetDuration);
}