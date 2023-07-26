namespace VoicerStudio.Application.Models.Speech;

public class SpeechGenerateResult
{
    public required Stream AudioData { get; set; }
    public required string MimeType { get; set; }
    public TimeSpan? Start { get; set; }
    public TimeSpan? InputDuration { get; set; }
    public required TimeSpan OutputDuration { get; set; }
}