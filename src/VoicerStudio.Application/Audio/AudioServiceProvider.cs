using NeerCore.Exceptions;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.Audio;

public sealed class AudioServiceProvider
{
    private readonly IEnumerable<IAudioService> _audioServices;

    public AudioServiceProvider(IEnumerable<IAudioService> audioServices)
    {
        _audioServices = audioServices;
    }


    public IAudioService GetService(AudioFormat audioFormat)
    {
        var audioService = _audioServices.FirstOrDefault(x => x.AudioFormat == audioFormat);
        return audioService
            ?? throw new ValidationFailedException($"Invalid audio format provided '{audioFormat.ToString().ToLower()}'");
    }
}