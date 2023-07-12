using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using VoicerStudio.Application.Extensions;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.Infrastructure;

internal sealed class AudioService : IAudioService
{
    private readonly ILogger<AudioService> _logger;
    private readonly AudioOptions _options;

    public AudioService(ILogger<AudioService> logger, IOptions<AudioOptions> optionsAccessor)
    {
        _logger = logger;
        _options = optionsAccessor.Value;
    }


    public async Task<byte[]> MergeAsync(byte[][] audioFiles, double[] offsetsInMilliseconds)
    {
        var buffer = new byte[4096]; // 1024 * 4
        WaveFileWriter? waveFileWriter = null;

        using var output = new MemoryStream();
        try
        {
            for (var i = 0; i < audioFiles.Length; i++)
            {
                var binaryData = audioFiles[i];
                var offset = offsetsInMilliseconds[i];
                using var audioStream = new MemoryStream(binaryData);
                await using var reader = new WaveFileReader(audioStream);

                if (waveFileWriter is null)
                    waveFileWriter = new WaveFileWriter(output, reader.WaveFormat);
                else if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                    throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");

                if (offset > 0) WriteSilence(waveFileWriter, offset);
                WriteFromBuffer(reader, waveFileWriter, buffer);
            }

            waveFileWriter?.Flush();
            return output.ToArray();
        }
        finally
        {
            // ReSharper disable once MethodHasAsyncOverload
            waveFileWriter?.Dispose();
        }
    }

    public async Task<ResizeResult> ResizeAsync(byte[] audioData, TimeSpan targetDuration)
    {
        var outputStream = new MemoryStream();
        var inputStream = new MemoryStream(audioData);

        var inputDuration = await GetAudioDurationAsync(inputStream);

        try
        {
            if (inputDuration == TimeSpan.Zero)
                throw new ArgumentException("Audio data is not valid", nameof(audioData));

            inputStream.Position = 0;

            var speed = CalcSpeed(inputDuration, targetDuration);

            var success = await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(inputStream))
                .OutputToPipe(new StreamPipeSink(outputStream), options => options
                    .WithAudioFilters(filter => filter.AddATempo(speed))
                    .ForceFormat("wav"))
                .ProcessAsynchronously();

            outputStream.Position = 0;
            var outputFileReader = new WaveFileReader(outputStream);
            var outputDuration = outputFileReader.TotalTime;

            outputStream.Position = 0;
            var outputFile = outputStream.ToArray();

            return new ResizeResult(
                Success: true,
                AudioData: outputFile,
                InputDuration: inputDuration,
                OutputDuration: outputDuration
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occured while executing FFMpeg");
        }
        finally
        {
            await inputStream.DisposeAsync();
            await outputStream.DisposeAsync();
        }

        return new ResizeResult(
            Success: false,
            AudioData: audioData,
            InputDuration: inputDuration,
            OutputDuration: inputDuration
        );
    }

    private static async Task<TimeSpan> GetAudioDurationAsync(Stream stream)
    {
        try
        {
            await using var waveFileReader = new WaveFileReader(stream);
            return waveFileReader.TotalTime;
        }
        catch (Exception)
        {
            return TimeSpan.Zero;
        }
    }

    private double CalcSpeed(TimeSpan originalDuration, TimeSpan targetDuration)
    {
        var speed = originalDuration.TotalMilliseconds / targetDuration.TotalMilliseconds;
        return Math.Clamp(speed, -_options.MaxSpeedDelta, _options.MaxSpeedDelta);
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