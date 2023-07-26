namespace VoicerStudio.Application.CognitiveServices.VoiceMaker.Models;

public record VoiceMakerErrorResponse(
    bool Success,
    string Message
);