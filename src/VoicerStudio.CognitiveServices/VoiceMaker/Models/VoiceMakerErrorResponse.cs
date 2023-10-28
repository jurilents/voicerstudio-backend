namespace VoicerStudio.CognitiveServices.VoiceMaker.Models;

public record VoiceMakerErrorResponse(
    bool Success,
    string Message
);