namespace VoicerStudio.Application.Models;

public class Subtitle
{
    public int RowId { get; set; }
    public Guid Id { get; set; }
    public int Speaker { get; set; }
    public string Text { get; set; }
    public string? Note { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
}