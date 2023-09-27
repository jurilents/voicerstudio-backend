namespace VoicerStudio.Application.Models.ConferenceTranslations;

public class CtResult<T>
{
    public required T Result { get; set; }
}

public class CtArrayResult<T>
{
    public required T[] Results { get; set; }
}