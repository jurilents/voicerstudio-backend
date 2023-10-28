using VoicerStudio.Application.Enums;

namespace VoicerStudio.Application.Services;

public interface IAudioServiceProvider
{
    IAudioService GetService(AudioFormat audioFormat);
}