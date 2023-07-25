using System.Globalization;
using FFMpegCore.Arguments;

namespace VoicerStudio.Application.CognitiveServices.VoiceMaker;

public class AtempoAudioFilter : IAudioFilterArgument
{
    public string Key => "atempo";
    public string Value { get; }


    public AtempoAudioFilter(double value)
    {
        Value = value.ToString("F5", CultureInfo.InvariantCulture);
    }
}