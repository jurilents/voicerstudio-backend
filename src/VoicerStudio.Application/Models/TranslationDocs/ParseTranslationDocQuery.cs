using Microsoft.AspNetCore.Mvc;

namespace VoicerStudio.Application.Models.TranslationDocs;

public class ParseTranslationDocQuery
{
    [FromQuery(Name = "lang")]
    public required string Language { get; init; }

    [FromQuery(Name = "langBase")]
    public required string BaseLanguages { get; init; }

    public required bool SplitByParagraph { get; init; }
}