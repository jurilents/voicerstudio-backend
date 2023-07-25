using FFMpegCore.Arguments;
using VoicerStudio.Application.CognitiveServices.VoiceMaker;

namespace VoicerStudio.Application.Extensions;

public static class AudioFilterOptionsExtensions
{
    public static AudioFilterOptions AddATempo(this AudioFilterOptions options, double atempo)
    {
        options.Arguments.Add(new AtempoAudioFilter(atempo));
        return options;
    }
}