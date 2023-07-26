using System.Text.Json.Serialization;

namespace VoicerStudio.Application.CognitiveServices.VoiceMaker.Models;

public class VoiceMakerSpeechResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    [JsonPropertyName("path")]
    public required string Path { get; init; }
}