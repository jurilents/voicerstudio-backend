using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VoicerStudio.Application.Azure;
using VoicerStudio.Application.Infrastructure;
using VoicerStudio.Application.Infrastructure.Repositories;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Repositories;
using VoicerStudio.Application.Services;
using VoicerStudio.Application.VoiceMaker;

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
    }

    private static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEncryptor, AesEncryptor>();
        services.AddScoped<IAudioService, AudioService>();
        services.AddScoped<ICredentialsService, AzureCredentialsService>();
        services.AddScoped<GoogleSheetsAccessor>();

        services.AddScoped<ISpeakerRepository, SpeakerRepository>();
        services.AddScoped<ISubtitleRepository, SubtitleRepository>();
    }

    private static void AddAzureCognitiveService(this IServiceCollection services)
    {
        services.AddScoped<ICognitiveService, AzureCognitiveService>();
    }


    private static void AddVoiceMakerCognitiveService(this IServiceCollection services)
    {
        services.AddScoped<ICognitiveService, VoiceMakerCognitiveService>();
        services.AddScoped<VoiceMakerService>();
        services.AddHttpClient<VoiceMakerService>((provider, httpClient) =>
        {
            var options = provider.GetRequiredService<IOptions<VoiceMakerOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.ApiUrl);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + options.ApiKey);
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