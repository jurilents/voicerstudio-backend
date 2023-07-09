namespace VoicerStudio.Application.Models;

public class Speaker
{
    public int RowId { get; set; }
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Preset { get; set; } = null!;
}