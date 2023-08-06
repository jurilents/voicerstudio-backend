using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using NeerCore.Exceptions;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.Audio.Wav;

internal class WavAudioService : IAudioService
{
    private readonly ILogger<WavAudioService> _logger;
    private readonly AudioOptions _options;

    public WavAudioService(ILogger<WavAudioService> logger, IOptions<AudioOptions> optionsAccessor)
    {
        _logger = logger;
        _options = optionsAccessor.Value;
    }


    public AudioFormat AudioFormat => AudioFormat.Wav;

    public async Task<Stream> MergeAsync(Stream[] audioFiles, double[] offsetsInMilliseconds)
    {
        var buffer = new byte[4096]; // 1024 * 4
        WaveFileWriter? waveFileWriter = null;

        var output = new MemoryStream();
        try
        {
            for (var i = 0; i < audioFiles.Length; i++)
            {
                await using var audioStream = audioFiles[i];
                var offset = offsetsInMilliseconds[i];
                await using var reader = new WaveFileReader(audioStream);

                if (waveFileWriter is null)
                    waveFileWriter = new WaveFileWriter(output, reader.WaveFormat);
                else if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                    throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");

                if (offset > 0) WriteSilence(waveFileWriter, offset);
                WriteFromBuffer(reader, waveFileWriter, buffer);
            }

            waveFileWriter?.Flush();
            output.Position = 0;
            return output;
        }
        finally
        {
            // ReSharper disable once MethodHasAsyncOverload
            // waveFileWriter?.Dispose();
        }
    }

    public async Task<ResizeResult> ResizeAsync(Stream audioFile, TimeSpan targetDuration)
    {
        await using var reader = new WaveFileReader(audioFile);
        var baseDuration = reader.TotalTime;
        var speedRatio = baseDuration / targetDuration;
        var targetRate = (int)(reader.WaveFormat.SampleRate * speedRatio);
        var outFormat = new WaveFormat(targetRate, reader.WaveFormat.BitsPerSample, reader.WaveFormat.Channels);

        using var resampler = new MediaFoundationResampler(reader, outFormat);
        resampler.ResamplerQuality = 60; // Adjust the quality of the resampler if needed

        var buffer = new byte[1024];
        int bytesRead;

        var outputStream = new MemoryStream();
        await using var writer = new WaveFileWriter(outputStream, resampler.WaveFormat);

        while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
            await writer.WriteAsync(buffer.AsMemory(0, bytesRead));

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
            var reader = new WaveFileReader(stream);
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


    private double CalcSpeed(TimeSpan originalDuration, TimeSpan targetDuration)
    {
        var speed = originalDuration.TotalMilliseconds / targetDuration.TotalMilliseconds;
        return Math.Clamp(speed, 1.0 - _options.MaxPostProcessingSpeedDelta, 1.0 + _options.MaxPostProcessingSpeedDelta);
    }

    private static void WriteSilence(WaveFileWriter waveFileWriter, double duration)
    {
        var bytesPerMillisecond = waveFileWriter.WaveFormat.AverageBytesPerSecond / 1000.0;
        var silenceLength = Convert.ToInt32(duration * bytesPerMillisecond);
        var silentBytes = new byte[silenceLength];
        waveFileWriter.Write(silentBytes, 0, silentBytes.Length);
    }

    private static void WriteFromBuffer(WaveFileReader reader, WaveFileWriter waveFileWriter, byte[] buffer)
    {
        int read;
        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            waveFileWriter.Write(buffer, 0, read);
    }
}