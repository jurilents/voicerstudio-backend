using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using NeerCore.Exceptions;
using VoicerStudio.Application.Infrastructure;
using VoicerStudio.Application.Models;

namespace VoicerStudio.Application.VoiceMaker;

public class VoiceMakerService : IDisposable
{
    private readonly HttpClient _httpClient;

    public VoiceMakerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
                }).ToArray()
            }).ToArray();
    }

    public async Task<string> GenerateSpeechUrlAsync(SpeechGenerateRequest request, string credentials)
    {
        // return "https://developer.voicemaker.in/uploads/16891614658327thaybx-voicemaker.in-speech.wav";
        AddAuthorizationHeader(credentials);
        var requestJson = JsonSerializer.Serialize(new
        {
            Engine = "neutral",
            VoiceId = request.Voice,
            LanguageCode = request.Locale,
            Text = request.Text,
            OutputFormat = request.OutputFormat.ToString().ToLower(),
            SampleRate = ((int)request.SampleRate).ToString(),
            Effect = request.Style,
            MasterSpeed = 0,
            MasterVolume = request.Volume.HasValue ? (int)Math.Round(request.Volume.Value * 100) : 0,
            MasterPitch = request.Pitch.HasValue ? (int)Math.Round(request.Pitch.Value * 200) : 0,
        });

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