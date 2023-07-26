using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VoicerStudio.Application.Audio;
using VoicerStudio.Application.Audio.Mp3;
using VoicerStudio.Application.Audio.Wav;
using VoicerStudio.Application.CognitiveServices;
using VoicerStudio.Application.CognitiveServices.Azure;
using VoicerStudio.Application.CognitiveServices.VoiceMaker;
using VoicerStudio.Application.Infrastructure;
using VoicerStudio.Application.Infrastructure.Repositories;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Repositories;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddAllOptions();
        services.AddInfrastructure();

        services.AddAzureCognitiveService();
        services.AddVoiceMakerCognitiveService();
        services.AddScoped<CredentialsServicesProvider>();
        services.AddScoped<CognitiveServicesProvider>();
    }

    private static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEncryptor, AesEncryptor>();

        services.AddScoped<IAudioService, WavAudioService>();
        services.AddScoped<IAudioService, Mp3AudioService>();
        services.AddScoped<AudioServiceProvider>();

        services.AddScoped<GoogleSheetsAccessor>();

        services.AddScoped<ISpeakerRepository, SpeakerRepository>();
        services.AddScoped<ISubtitleRepository, SubtitleRepository>();
    }

    private static void AddAzureCognitiveService(this IServiceCollection services)
    {
        services.AddScoped<ICognitiveService, AzureCognitiveService>();
        services.AddScoped<ICredentialsService, AzureCredentialsService>();
    }


    private static void AddVoiceMakerCognitiveService(this IServiceCollection services)
    {
        // services.AddScoped<ICognitiveService, VoiceMakerCognitiveService>();
        services.AddScoped<VoiceMakerService>();
        services.AddScoped<ICredentialsService, VoiceMakerCredentialsService>();
        services.AddHttpClient<VoiceMakerService>((provider, httpClient) =>
        {
            var options = provider.GetRequiredService<IOptions<VoiceMakerOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.ApiUrl);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        });
    }

    private static void AddAllOptions(this IServiceCollection services)
    {
        services.AddOptions<AudioOptions>().BindConfiguration("Audio");
        services.AddOptions<AzureOptions>().BindConfiguration("Azure");
        services.AddOptions<VoiceMakerOptions>().BindConfiguration("VoiceMaker");
        services.AddOptions<GoogleOptions>().BindConfiguration("Google");
    }
}