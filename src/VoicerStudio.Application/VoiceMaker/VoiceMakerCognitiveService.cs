using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeerCore.Exceptions;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.VoiceMaker;

public class VoiceMakerCognitiveService : ICognitiveService, IDisposable
{
    private const string CacheKey = "vm_langs";

    private readonly ILogger<VoiceMakerCognitiveService> _logger;
    private readonly VoiceMakerService _voiceMakerService;
    private readonly AzureOptions _azure;
    private readonly IMemoryCache _memoryCache;
    private readonly IAudioService _audioService;

    public VoiceMakerCognitiveService(
        ILogger<VoiceMakerCognitiveService> logger, IOptions<AzureOptions> optionsAccessor,
        VoiceMakerService voiceMakerService, IMemoryCache memoryCache, IAudioService audioService)
    {
        _logger = logger;
        _voiceMakerService = voiceMakerService;
        _memoryCache = memoryCache;
        _audioService = audioService;
        _azure = optionsAccessor.Value;
    }


    public CognitiveServiceName ServiceName => CognitiveServiceName.VoiceMaker;

    public async Task<Language[]> GetLanguagesAsync(string credentials)
    {
        var languages = await _memoryCache.GetOrCreateAsync(
            CacheKey,
            async _ => await _voiceMakerService.GetLanguagesAsync(credentials));
        return languages!;
    }

    public async Task<SpeechGenerateResult> GenerateSpeechAsync(SpeechGenerateRequest request, string credentials)
    {
        var originalBytes = await _voiceMakerService.GenerateSpeechAsync(request, credentials);

        var result = request.Duration.HasValue
            ? await _audioService.ResizeAsync(originalBytes, request.Duration.Value)
            : throw new ValidationFailedException("Cannot calculate duration of output audio");

        return new SpeechGenerateResult
        {
            AudioData = result.AudioData,
            MimeType = "audio/wav",
            InputDuration = result.InputDuration,
            OutputDuration = result.OutputDuration,
        };
    }

    public async Task<SpeechGenerateResult> GenerateSpeechAsync(SpeechGenerateRequest[] requests, string credentials)
    {
        requests = requests.OrderBy(x => x.Start).ToArray();
        if (!ValidateBatchRequest(requests, out var overlaps))
            throw new ValidationFailedException("One or more speeches overlaps", overlaps);

        var results = new ConcurrentBag<SpeechGenerateResult>();
        await Parallel.ForEachAsync(requests, async (request, _) =>
        {
            var result = await GenerateSpeechAsync(request, credentials);
            result.Start = request.Start;
            results.Add(result);
        });

        // Get audio data
        var audioFiles = results
            .OrderBy(x => x.Start)
            .Select(x => x.AudioData).ToArray();

        // Calculate offsets
        var offsetsInMilliseconds = new double[requests.Length];
        if (!requests[0].Start.HasValue)
            throw new ValidationFailedException("Batch request 'start' and 'end' time range are required");
        offsetsInMilliseconds[0] = requests[0].Start!.Value.TotalMilliseconds;
        for (var i = 1; i < requests.Length; i++)
        {
            if (!requests[i].Start.HasValue || !requests[i - 1].End.HasValue)
                throw new ValidationFailedException("Batch request 'start' and 'end' time range are required");
            offsetsInMilliseconds[i] = (requests[i].Start!.Value - requests[i - 1].End!.Value)!.TotalMilliseconds;
        }

        // Merge all audio files with offsets
        var resultFile = await _audioService.MergeAsync(audioFiles, offsetsInMilliseconds);

        return new SpeechGenerateResult
        {
            AudioData = resultFile,
            MimeType = results.First().MimeType,
            OutputDuration = TimeSpan.FromSeconds(results.Sum(x => x.OutputDuration.TotalSeconds) + offsetsInMilliseconds.Sum()),
        };
    }

    private static bool ValidateBatchRequest(IReadOnlyList<SpeechGenerateRequest> requests, out Dictionary<string, object> overlaps)
    {
        overlaps = new Dictionary<string, object>();
        for (var i = 1; i < requests.Count; i++)
            if (requests[i].Start < requests[i - 1].End)
                overlaps.Add($"{requests[i - 1].Start} - {requests[i - 1].End}", $"{requests[i].Start} - {requests[i].End}");
        return overlaps.Count == 0;
    }


    public void Dispose()
    {
        _voiceMakerService.Dispose();
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }
}