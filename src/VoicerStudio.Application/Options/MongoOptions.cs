namespace VoicerStudio.Application.Options;

public class MongoOptions
{
    public string ConnectionString { get; init; } = null!;
    public string DatabaseName { get; init; } = null!;
}