using System.Collections.Concurrent;
using VoicerStudio.Application.Models.Recognition;
using VoicerStudio.Application.Services;
using Whisper.net;
using Whisper.net.Ggml;

namespace VoicerStudio.Infrastructure.Services;

public class SpeechRecognitionService : ISpeechRecognitionService
{
    private const string ModelName = "ggml-lg-v3.bin";
    private const GgmlType ModelType = GgmlType.LargeV3;


    public async Task<RecognitionResult> RecognizeAsync(Stream audioFile, CancellationToken ct = default)
    {
        await EnsureGgmlModelDownloadedAsync(ct);

        using var whisperFactory = WhisperFactory.FromPath(ModelName);

        await using var processor = whisperFactory.CreateBuilder()
            .WithTranslate()
            .WithLanguage("auto")
            .Build();

        var items = new ConcurrentBag<RecognitionResultItem>();
        await using var fileStream = File.OpenRead("yourAudio.wav");
        await foreach (var segment in processor.ProcessAsync(audioFile, ct))
        {
            Console.WriteLine($"CSSS {segment.Start} ==> {segment.End} : {segment.Text}");
            items.Add(new RecognitionResultItem(segment.Start, segment.End, segment.Text));
        }

        var resultItems = items.OrderBy(x => x.From).ToArray();
        return new RecognitionResult
        {
            Items = resultItems,
        };
    }

    private static async Task EnsureGgmlModelDownloadedAsync(CancellationToken ct)
    {
        if (!File.Exists(ModelName))
        {
            await using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ModelType, cancellationToken: ct);
            await using var fileWriter = File.OpenWrite(ModelName);
            await modelStream.CopyToAsync(fileWriter, ct);
        }
    }
}