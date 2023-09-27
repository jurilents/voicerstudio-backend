namespace VoicerStudio.Application.Models.ConferenceTranslations;

public class EventModel
{
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string Date { get; set; }
    public required LanguageModel[] Languages { get; set; }


    public static EventModel FromCtDto(CtEventDto dto) => new()
    {
        Title = dto.Title,
        Slug = dto.Slug,
        Date = dto.Date,
        Languages = dto.Langs.Select(LanguageModel.FromCtDto).ToArray()
    };
}

public class CtEventDto
{
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string Date { get; set; }
    public required CtLanguageDto[] Langs { get; set; }
}