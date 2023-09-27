using VoicerStudio.Application.Models.ConferenceTranslations;

namespace VoicerStudio.Application.Models.TranslationDocs;

public class ParseTranslationDocResult
{
    // public CtLanguageDto Lang { get; set; } = null!;
    public List<TextTranslationModel> Translations { get; set; } = new();
}

public class TextTranslationModel
{
    public required string Original { get; set; }
    public required string Translation { get; set; }
    // public required string? SpeakerName { get; set; }


    public void Normalize()
    {
        Original = Original.Trim();
        Translation = Translation.Trim();
        // SpeakerName = SpeakerName?.Trim();
    }

    public bool IsValid() => !string.IsNullOrEmpty(Original) && !string.IsNullOrEmpty(Translation);
}