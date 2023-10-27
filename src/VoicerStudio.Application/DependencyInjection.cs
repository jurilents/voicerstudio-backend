using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
        services.AddConferenceTranslationsApi();
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

        // services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ISpeakerRepository, SpeakerRepository>();
        services.AddScoped<ISubtitleRepository, SubtitleRepository>();

        services.AddScoped<ITranslationDocsParser, TranslationDocsParser>();
        services.AddScoped<ITranslateService, DeeplTranslateService>();
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

    private static void AddConferenceTranslationsApi(this IServiceCollection services)
    {
        // ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        // System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        // services.AddScoped<IConferenceTranslationsService, ConferenceTranslationsService>();
        services.AddHttpClient<IConferenceTranslationsService, ConferenceTranslationsService>((provider, httpClient) =>
        {
            var options = provider.GetRequiredService<IOptions<ConferenceTranslationsOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.ApiUrl);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
        }).ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13,
            ClientCertificates =
            {
                GetMyX509Certificate()
            },
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
        });
    }

    private static X509Certificate2 GetMyX509Certificate()
    {
        var certPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "ssl/certificate.pem");
        using var cert = X509Certificate.CreateFromCertFile(certPath);
        return new X509Certificate2(cert);
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