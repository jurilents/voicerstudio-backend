namespace VoicerStudio.Application.Models.ConferenceTranslations;

public class LanguageModel
{
    public required string Code { get; init; }
    public required string Name { get; init; }


    public static LanguageModel FromCtDto(CtLanguageDto dto) => new()
    {
        Code = dto.LangCode,
        Name = dto.LangName
    };
}

public class CtLanguageDto
{
    public required string LangCode { get; init; }
    public required string LangName { get; init; }
}