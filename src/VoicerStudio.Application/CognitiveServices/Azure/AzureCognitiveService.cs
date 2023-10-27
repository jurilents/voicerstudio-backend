using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeerCore.Exceptions;
using VoicerStudio.Application.Audio;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Infrastructure;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Models.Speech;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.CognitiveServices.Azure;

public class AzureCognitiveService : ICognitiveService
{
    private const string CacheKey = "azure_langs_";

    private readonly AzureOptions _azure;
    private readonly IMemoryCache _memoryCache;
    private readonly AudioServiceProvider _audioServices;
    private readonly ILogger<AzureCognitiveService> _logger;
    private readonly CredentialsServicesProvider _credentialsServices;

    public AzureCognitiveService(
        ILogger<AzureCognitiveService> logger, IOptions<AzureOptions> optionsAccessor,
        IMemoryCache memoryCache, CredentialsServicesProvider credentialsServices, AudioServiceProvider audioServices)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _azure = optionsAccessor.Value;
        _credentialsServices = credentialsServices;
        _audioServices = audioServices;
    }


    public CognitiveServiceName ServiceName => CognitiveServiceName.Azure;

    private ICredentialsService CredentialsService => _credentialsServices.GetService(ServiceName);

    public async Task<LanguageWithVoices[]> GetLanguagesAsync(string credentials)
    {
        var languages = await _memoryCache.GetOrCreateAsync(
            CacheKey + _azure.Credentials.Region + credentials,
            async _ => await GetLanguagesInternalAsync(credentials));
        return languages!;
    }

    public async Task<SpeechGenerateResult> GenerateSpeechAsync(SpeechGenerateRequest request, string credentials)
    {
        var cred = await CredentialsService.UnsecureAsync(credentials);
        var config = SpeechConfig.FromSubscription(cred["subscriptionKey"], cred["region"]);
        config.SpeechSynthesisLanguage = request.Locale;
        config.SpeechSynthesisVoiceName = request.Voice;
        var synthesisFormat = GetSpeechOutputFormat(request.OutputFormat, request.SampleRate);
        config.SetSpeechSynthesisOutputFormat(synthesisFormat); //Raw48Khz16BitMonoPcm

        using var synthesizer = new SpeechSynthesizer(config, null);
        var ssml = SsmlBuilder.BuildSsml(request);
        _logger.LogInformation("Result SSML {Code}", ssml);
        var result = await synthesizer.SpeakSsmlAsync(ssml);

        if (result.AudioData.Length == 0)
        {
            _logger.LogWarning("Empty content generated {Req}", JsonSerializer.Serialize(request));
            throw new ValidationFailedException("Empty content generated");
        }

        return new SpeechGenerateResult
        {
            AudioData = new MemoryStream(result.AudioData),
            MimeType = "audio/" + request.OutputFormat.ToString().ToLower(),
            InputDuration = request.Speed == 0 ? result.AudioDuration : null,
            OutputDuration = result.AudioDuration,
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
        var audioFormat = requests.First().OutputFormat;
        var audioService = _audioServices.GetService(audioFormat);
        var resultFile = await audioService.MergeAsync(audioFiles, offsetsInMilliseconds);

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

    private async Task<LanguageWithVoices[]> GetLanguagesInternalAsync(string credentials)
    {
        var cred = await CredentialsService.UnsecureAsync(credentials);
        var config = SpeechConfig.FromSubscription(cred["subscriptionKey"], cred["region"]);
        using var synthesizer = new SpeechSynthesizer(config);
        using var result = await synthesizer.GetVoicesAsync();

        if (result.Reason == ResultReason.VoicesListRetrieved)
            _logger.LogInformation("Found {Count} voices", result.Voices.Count);
        else if (result.Voices.Count == 0)
        {
            var message = $"Voices did not fetch due to an error: {result.Reason}";
            _logger.LogError(message);
            throw new InternalServerException(message);
        }

        return result.Voices
            .GroupBy(voice => voice.Locale)
            .Select(voices => new LanguageWithVoices
            {
                Locale = voices.Key,
                DisplayName = CultureNormalizer.GetCultureDisplayName(voices.Key),
                Voices = voices.Select(voice =>
                {
                    var displayName = voice.Properties.GetProperty("DisplayName");
                    var words = voice.Properties.GetProperty("WordsPerMinute");
                    var roles = voice.Properties.GetProperty("RolePlayList");
                    return new Voice
                    {
                        Key = voice.ShortName,
                        DisplayName = voice.LocalName != displayName ? $"{voice.LocalName} ({displayName})" : displayName,
                        Gender = voice.Gender.ToString(),
                        Type = voice.VoiceType.ToString(),
                        WordsPerMinute = int.TryParse(words, out var v) ? v : 150,
                        Styles = voice.StyleList.Where(style => !string.IsNullOrEmpty(style)).ToArray(),
                        Roles = string.IsNullOrEmpty(roles) ? Array.Empty<string>() : JsonSerializer.Deserialize<string[]>(roles),
                    };
                }).ToArray()
            })
            .OrderBy(lang => lang.DisplayName)
            .ToArray();
    }

    private static SpeechSynthesisOutputFormat GetSpeechOutputFormat(AudioFormat outputFormat, AudioSampleRate sampleRate) =>
        (outputFormat, sampleRate) switch
        {
            // (AudioFormat.Wav, AudioSampleRate.Rate8000)  => SpeechSynthesisOutputFormat.Riff8Khz16BitMonoPcm,
            (AudioFormat.Wav, AudioSampleRate.Rate16000) => SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm,
            (AudioFormat.Wav, AudioSampleRate.Rate24000) => SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm,
            (AudioFormat.Wav, AudioSampleRate.Rate48000) => SpeechSynthesisOutputFormat.Riff48Khz16BitMonoPcm,
            (AudioFormat.Mp3, AudioSampleRate.Rate16000) => SpeechSynthesisOutputFormat.Audio16Khz128KBitRateMonoMp3,
            (AudioFormat.Mp3, AudioSampleRate.Rate24000) => SpeechSynthesisOutputFormat.Audio24Khz160KBitRateMonoMp3,
            (AudioFormat.Mp3, AudioSampleRate.Rate48000) => SpeechSynthesisOutputFormat.Audio48Khz192KBitRateMonoMp3,
            _                                            => throw new ValidationFailedException("Invalid output format provided")
        };
}