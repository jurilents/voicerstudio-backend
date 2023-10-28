namespace VoicerStudio.Application.Models;

public record Error(
    int Code,
    string Message,
    Dictionary<string, string>? Errors = null
);