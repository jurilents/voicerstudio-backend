using Microsoft.Extensions.DependencyInjection;
using VoicerStudio.Application.Services;
using VoicerStudio.Infrastructure.Audio;
using VoicerStudio.Infrastructure.Audio.Mp3;
using VoicerStudio.Infrastructure.Audio.Wav;
using VoicerStudio.Infrastructure.Services;

namespace VoicerStudio.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEncryptor, AesEncryptor>();

        services.AddScoped<IAudioService, WavAudioService>();
        services.AddScoped<IAudioService, Mp3AudioService>();
        services.AddScoped<AudioServiceProvider>();

        services.AddScoped<ITranslateService, DeeplTranslateService>();
    }
}