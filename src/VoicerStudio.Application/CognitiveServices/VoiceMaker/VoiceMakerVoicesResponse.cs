using System.Text.Json.Serialization;

namespace VoicerStudio.Application.CognitiveServices.VoiceMaker;

public class VoiceMakerVoicesResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("data")]
    public required VoiceMakerVoicesResponseData Data { get; init; }
}

public class VoiceMakerVoicesResponseData
{
    [JsonPropertyName("voices_list")]
    public required VoiceMakerVoice[] VoiceList { get; init; }
}

public class VoiceMakerVoice
{
    // neural
    public required string Engine { get; init; }

    // ai3-en-US-Alexander
    public required string VoiceId { get; init; }

    // Male
    public required string VoiceGender { get; init; }

    // Alexander
    public required string VoiceWebname { get; init; }

    // US
    public required string Country { get; init; }

    // en-US
    public required string Language { get; init; }

    // English, US
    public required string LanguageName { get; init; }

    // conversational, excited, friendly...
    public required string[] VoiceEffects { get; init; }
}