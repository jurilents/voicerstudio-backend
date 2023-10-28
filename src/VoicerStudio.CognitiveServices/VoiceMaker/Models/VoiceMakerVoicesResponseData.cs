using System.Text.Json.Serialization;

namespace VoicerStudio.CognitiveServices.VoiceMaker.Models;

public class VoiceMakerVoicesResponseData
{
    [JsonPropertyName("voices_list")]
    public required VoiceMakerVoice[] VoiceList { get; init; }
}