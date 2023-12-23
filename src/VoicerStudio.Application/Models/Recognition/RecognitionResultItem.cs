namespace VoicerStudio.Application.Models.Recognition;

public record RecognitionResultItem(
    TimeSpan From,
    TimeSpan To,
    string Text
);