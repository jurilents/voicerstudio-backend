using System.Text.Json.Serialization;

namespace VoicerStudio.CognitiveServices.VoiceMaker.Models;

public class VoiceMakerVoicesResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("data")]
    public required VoiceMakerVoicesResponseData Data { get; init; }
}