using NAudio.Wave;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.Infrastructure;

internal sealed class AudioService : IAudioService
{
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