using Microsoft.Extensions.DependencyInjection;
using VoicerStudio.Application.Options;

namespace VoicerStudio.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddAllOptions();
    }

    private static void AddAllOptions(this IServiceCollection services)
    {
        services.AddOptions<AudioOptions>().BindConfiguration("Audio");
        services.AddOptions<AzureOptions>().BindConfiguration("Azure");
        services.AddOptions<DeeplOptions>().BindConfiguration("Deepl");
        services.AddOptions<GoogleOptions>().BindConfiguration("Google");
        services.AddOptions<MongoOptions>().BindConfiguration("Mongo");
        services.AddOptions<SecurityOptions>().BindConfiguration("Security");
        services.AddOptions<TelegramOptions>().BindConfiguration("Telegram");
        services.AddOptions<VoiceMakerOptions>().BindConfiguration("VoiceMaker");
    }
}