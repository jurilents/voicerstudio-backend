namespace VoicerStudio.Application.Models;

public class TranslateRequest
{
    public required string[] Texts { get; set; }
    public required string SourceLang { get; set; }
    public required string TargetLang { get; set; }
}