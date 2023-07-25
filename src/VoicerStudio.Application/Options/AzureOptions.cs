namespace VoicerStudio.Application.Options;

public class AzureOptions
{
    public required AzureCredentialsOptions User { get; init; }
    public required AzureCredentialsOptions Credentials { get; init; }
}

public class AzureCredentialsOptions
{
    public string SubscriptionKey { get; init; } = null!;
    public string Region { get; init; } = null!;


    public static bool operator ==(AzureCredentialsOptions? a, AzureCredentialsOptions? b) =>
        a is not null
        && b is not null
        && a.Region == b.Region
        && a.SubscriptionKey == b.SubscriptionKey;

    public static bool operator !=(AzureCredentialsOptions? a, AzureCredentialsOptions? b) => !(a == b);

    protected bool Equals(AzureCredentialsOptions other)
    {
        return SubscriptionKey == other.SubscriptionKey
            && Region == other.Region;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AzureCredentialsOptions)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SubscriptionKey, Region);
    }
}