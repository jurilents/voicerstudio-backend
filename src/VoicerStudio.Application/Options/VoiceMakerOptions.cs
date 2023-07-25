namespace VoicerStudio.Application.Options;

public class VoiceMakerOptions
{
    public string ApiUrl { get; init; } = null!;
    public VoiceMakerCredentialsOptions Credentials { get; init; } = null!;
    public VoiceMakerCredentialsOptions User { get; init; } = null!;
}

public class VoiceMakerCredentialsOptions
{
    public string ApiKey { get; init; } = null!;
}