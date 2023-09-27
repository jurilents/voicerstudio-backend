namespace VoicerStudio.Application.Models;

public class Language
{
    public string Locale { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
}

public class LanguageWithVoices : Language
{
    public Voice[] Voices { get; set; } = null!;
}