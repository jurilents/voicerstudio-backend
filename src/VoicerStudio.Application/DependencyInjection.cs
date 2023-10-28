using Microsoft.Extensions.DependencyInjection;
using VoicerStudio.Application.Audio;
using VoicerStudio.Application.Audio.Mp3;
using VoicerStudio.Application.Audio.Wav;
using VoicerStudio.Application.Infrastructure;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddAllOptions();
        services.AddInfrastructure();
    }

    private static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEncryptor, AesEncryptor>();

        services.AddScoped<IAudioService, WavAudioService>();
        services.AddScoped<IAudioService, Mp3AudioService>();
        services.AddScoped<AudioServiceProvider>();

        services.AddScoped<ITranslateService, DeeplTranslateService>();
    }


    private static void AddAllOptions(this IServiceCollection services)
    {
        services.AddOptions<AudioOptions>().BindConfiguration("Audio");
        services.AddOptions<AzureOptions>().BindConfiguration("Azure");
        services.AddOptions<ConferenceTranslationsOptions>().BindConfiguration("ConferenceTranslations");
        services.AddOptions<DeeplOptions>().BindConfiguration("Deepl");
        services.AddOptions<GoogleOptions>().BindConfiguration("Google");
        services.AddOptions<MongoOptions>().BindConfiguration("Mongo");
        services.AddOptions<TelegramOptions>().BindConfiguration("Telegram");
        services.AddOptions<VoiceMakerOptions>().BindConfiguration("VoiceMaker");
    }
}