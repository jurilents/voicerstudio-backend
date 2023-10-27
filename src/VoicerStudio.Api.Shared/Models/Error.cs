namespace VoicerStudio.Api.Shared.Models;

public record Error(
    int Code,
    string Message,
    Dictionary<string, string>? Errors = null
);