using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Lame;
using NAudio.Wave;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Infrastructure.Audio.Mp3;

internal class Mp3AudioService : IAudioService
{
    private readonly ILogger<Mp3AudioService> _logger;
    private readonly AudioOptions _options;

    public Mp3AudioService(ILogger<Mp3AudioService> logger, IOptions<AudioOptions> optionsAccessor)
    {
        _logger = logger;
        _options = optionsAccessor.Value;
    }


    public AudioFormat AudioFormat => AudioFormat.Mp3;

    public Task<Stream> MergeAsync(Stream[] audioFiles, double[] offsetsInMilliseconds)
    {
        throw new NotImplementedException();
    }

    public async Task<ResizeResult> ResizeAsync(Stream audioFile, TimeSpan targetDuration)
    {
        await using var reader = new Mp3FileReader(audioFile);
        var baseDuration = reader.TotalTime;
        var speedRatio = baseDuration / targetDuration;
        var targetRate = (int)(reader.WaveFormat.SampleRate * speedRatio);
        var outFormat = new WaveFormat(targetRate, reader.Mp3WaveFormat.BitsPerSample, reader.Mp3WaveFormat.Channels);

        using var resampler = new MediaFoundationResampler(reader, outFormat);
        resampler.ResamplerQuality = 60; // Adjust the quality of the resampler if needed

        // Convert the resampled audio to PCM and use LAME encoder to write it to the output stream
        var outputStream = new MemoryStream();
        await using (var mp3Writer = new LameMP3FileWriter(outputStream, resampler.WaveFormat, 128))
        {
            var buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                await mp3Writer.WriteAsync(buffer.AsMemory(0, bytesRead));
        }

        outputStream.Position = 0;
        return new ResizeResult(
            Audio: outputStream,
            InputDuration: baseDuration,
            OutputDuration: baseDuration * speedRatio
        );
    }

    public TimeSpan GetAudioDuration(Stream stream)
    {
        try
        {
            stream.Position = 0;
            var reader = new Mp3FileReader(stream);
            return reader.TotalTime;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Cannot read audio duration");
            return TimeSpan.Zero;
        }
        finally
        {
            stream.Position = 0;
        }
    }
}