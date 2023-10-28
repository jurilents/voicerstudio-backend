namespace VoicerStudio.Application.Options;

public class SecurityOptions
{
    public string JwtSecret { get; init; } = null!;
    public TimeSpan TokenLifetime { get; init; }
    public short UserTokenLength { get; init; }
}