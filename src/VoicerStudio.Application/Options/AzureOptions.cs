namespace VoicerStudio.Application.Options;

public class AzureOptions
{
    public string SubscriptionKey { get; set; } = null!;
    public string Region { get; set; } = null!;


    public static bool operator ==(AzureOptions? a, AzureOptions? b) =>
        a is not null
        && b is not null
        && a.Region == b.Region
        && a.SubscriptionKey == b.SubscriptionKey;

    public static bool operator !=(AzureOptions? a, AzureOptions? b) => !(a == b);
}