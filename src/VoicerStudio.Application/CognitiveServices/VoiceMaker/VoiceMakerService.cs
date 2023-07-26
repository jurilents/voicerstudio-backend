using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeerCore.Exceptions;
using VoicerStudio.Application.CognitiveServices.VoiceMaker.Models;
using VoicerStudio.Application.Infrastructure;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Models.Speech;
using VoicerStudio.Application.Options;

namespace VoicerStudio.Application.CognitiveServices.VoiceMaker;

public class VoiceMakerService : IDisposable
{
    private readonly ILogger<VoiceMakerService> _logger;
    private readonly HttpClient _httpClient;
    private readonly AudioOptions _options;

    public VoiceMakerService(ILogger<VoiceMakerService> logger, HttpClient httpClient, IOptions<AudioOptions> audioOptionsAccessor)
    {
        _logger = logger;
        _httpClient = httpClient;
        _options = audioOptionsAccessor.Value;
    }


    public async Task<Language[]> GetLanguagesAsync(string credentials)
    {
        AddAuthorizationHeader(credentials);
        var response = await _httpClient.PostAsync("/voice/list", new StringContent(""));
        if (response.StatusCode != HttpStatusCode.OK)
            throw new ValidationFailedException("Cannot load VoiceMaker languages");

        var result = await response.Content.ReadFromJsonAsync<VoiceMakerVoicesResponse>();
        if (result is null || !result.Success)
            throw new ValidationFailedException("Loading VoiceMaker languages failed");

        return result.Data.VoiceList
            .GroupBy(voice => voice.Language)
            .OrderBy(voice => voice.Key)
            .Select(voices => new Language
            {
                Locale = voices.Key,
                DisplayName = CultureNormalizer.GetCultureDisplayName(voices.Key),
                Voices = voices.Select(voice => new Voice
                {
                    Key = voice.VoiceId,
                    DisplayName = voice.VoiceWebname,
                    Gender = voice.VoiceGender,
                    Type = voice.Engine,
                    WordsPerMinute = 150,
                    Styles = voice.VoiceEffects,
                }).OrderBy(voice => voice.DisplayName).ToArray()
            }).ToArray();
    }

    public async Task<string> GenerateSpeechUrlAsync(SpeechGenerateRequest request, string credentials)
    {
        AddAuthorizationHeader(credentials);

        request.Speed = request.Speed.HasValue ? Math.Clamp(request.Speed.Value, 0.5, 1.5) : 1.0;
        // return "https://developer.voicemaker.in/uploads/16891614658327thaybx-voicemaker.in-speech.wav";
        // AddAuthorizationHeader(credentials);
        var masterSpeed = Math.Clamp(1.0 - request.Speed.Value, -_options.MaxServiceSpeedDelta, _options.MaxServiceSpeedDelta) * 100;
        var masterVolume = request.Volume.HasValue ? (int)Math.Round(request.Volume.Value * 100) : 0;
        var masterPitch = request.Pitch.HasValue ? (int)Math.Round(request.Pitch.Value * 200) : 0;

        var requestJson = JsonSerializer.Serialize(new
        {
            Engine = "neural",
            VoiceId = request.Voice,
            LanguageCode = request.Locale,
            Text = request.Text,
            OutputFormat = request.OutputFormat.ToString().ToLower(),
            SampleRate = ((int)request.SampleRate).ToString(),
            Effect = string.IsNullOrEmpty(request.Style) ? "default" : request.Style,
            MasterSpeed = ((int)Math.Round(masterSpeed)).ToString(CultureInfo.InvariantCulture),
            MasterVolume = masterVolume.ToString(CultureInfo.InvariantCulture),
            MasterPitch = masterPitch.ToString(CultureInfo.InvariantCulture),
        });

        _logger.LogDebug("VoiceMaker request payload: {@Json}", requestJson);

        var content = new StringContent(requestJson, Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await _httpClient.PostAsync("/voice/api", content);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadFromJsonAsync<VoiceMakerErrorResponse>();
            throw new ValidationFailedException($"Cannot voice text on VoiceMaker: {error?.Message}");
        }
        var result = await response.Content.ReadFromJsonAsync<VoiceMakerSpeechResponse>();
        if (result is null || !result.Success)
            throw new ValidationFailedException("Voicing text on VoiceMaker failed");
        return result.Path;
    }

    public async Task<byte[]> GenerateSpeechAsync(SpeechGenerateRequest request, string credentials)
    {
        var audioUrl = await GenerateSpeechUrlAsync(request, credentials);

        var resultAudioBytes = await _httpClient.GetByteArrayAsync(audioUrl);
        if (resultAudioBytes.Length == 0)
            throw new ValidationFailedException("Empty audio received");

        return resultAudioBytes;
    }

    private void AddAuthorizationHeader(string credentials)
    {
        if (string.IsNullOrEmpty(credentials)) return;
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + credentials);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}