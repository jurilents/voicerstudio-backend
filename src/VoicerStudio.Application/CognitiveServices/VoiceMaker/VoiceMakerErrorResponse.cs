namespace VoicerStudio.Application.CognitiveServices.VoiceMaker;

public record VoiceMakerErrorResponse(
    bool Success,
    string Message
);