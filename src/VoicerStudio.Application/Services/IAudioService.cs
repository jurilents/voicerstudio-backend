using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Infrastructure;

namespace VoicerStudio.Application.Services;

public record ResizeResult(
    byte[] AudioData,
    TimeSpan InputDuration,
    TimeSpan OutputDuration,
    bool Success = true
);

public interface IAudioService
{
    Task<byte[]> MergeAsync(byte[][] audioFiles, double[] offsetsInMilliseconds);
    Task<ResizeResult> ResizeAsync(byte[] audioFile, TimeSpan targetDuration, AudioFormat audioFormat);
    TimeSpan GetAudioDuration(Stream stream, AudioFormat audioFormat);
}