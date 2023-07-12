namespace VoicerStudio.Application.Options;

public class AzureOptions
{
    public required AzureCredentialsOptions User { get; init; }
    public required AzureCredentialsOptions Credentials { get; init; }
}