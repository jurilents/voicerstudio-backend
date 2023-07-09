namespace VoicerStudio.Application.Services;

public interface IAudioService
{
    Task<byte[]> MergeAsync(byte[][] audioFiles, double[] offsetsInMilliseconds);
}