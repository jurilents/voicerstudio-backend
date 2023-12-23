using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;
using VoicerStudio.CognitiveServices.Azure;
using VoicerStudio.CognitiveServices.Shared;
using VoicerStudio.CognitiveServices.VoiceMaker;

namespace VoicerStudio.CognitiveServices;

public static class DependencyInjection
{
    public static void AddCognitiveServices(this IServiceCollection services)
    {
        services.AddTelegramAuthorization();
        services.AddAzureCognitiveService();
        services.AddVoiceMakerCognitiveService();

        services.AddScoped<CredentialsServicesProvider>();
        services.AddScoped<CognitiveServicesProvider>();
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


    private static void AddTelegramAuthorization(this IServiceCollection services)
    {
        services.AddScoped<TelegramBotAuthorizer>();
        services.AddScoped<ICredentialsService, TelegramBotCredentials>();
    }
}