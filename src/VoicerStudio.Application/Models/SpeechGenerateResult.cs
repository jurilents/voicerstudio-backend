namespace VoicerStudio.Application.Models;

public class SpeechGenerateResult
{
    public byte[] AudioData { get; set; }
    public string MimeType { get; set; }
    public TimeSpan? Start { get; set; }
    public TimeSpan Duration { get; set; }
}