using FFMpegCore.Arguments;
using VoicerStudio.Application.VoiceMaker;

namespace VoicerStudio.Application.Extensions;

public static class AudioFilterOptionsExtensions
{
    public static void AddATempo(this AudioFilterOptions options, double atempo)
    {
        options.Arguments.Add(new AtempoAudioFilter(atempo));
    }
}