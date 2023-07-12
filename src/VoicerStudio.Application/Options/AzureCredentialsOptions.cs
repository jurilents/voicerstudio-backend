namespace VoicerStudio.Application.Options;

public class AzureCredentialsOptions
{
    public string SubscriptionKey { get; set; } = null!;
    public string Region { get; set; } = null!;


    public static bool operator ==(AzureCredentialsOptions? a, AzureCredentialsOptions? b) =>
        a is not null
        && b is not null
        && a.Region == b.Region
        && a.SubscriptionKey == b.SubscriptionKey;

    public static bool operator !=(AzureCredentialsOptions? a, AzureCredentialsOptions? b) => !(a == b);
}