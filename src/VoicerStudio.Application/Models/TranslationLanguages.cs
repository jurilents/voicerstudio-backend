namespace VoicerStudio.Application.Models;

public class TranslationLanguages
{
    public required Language[] SourceLanguages { get; set; }
    public required Language[] TargetLanguages { get; set; }
}