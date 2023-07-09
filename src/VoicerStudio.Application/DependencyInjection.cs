using Microsoft.Extensions.DependencyInjection;
using VoicerStudio.Application.Azure;
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

    private static void AddAllOptions(this IServiceCollection services)
    {
        services.AddOptions<CredentialsOptions>().BindConfiguration("Credentials");
        services.AddOptions<GoogleOptions>().BindConfiguration("Google");
    }
}