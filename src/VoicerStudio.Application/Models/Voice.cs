namespace VoicerStudio.Application.Models;

public class Voice
{
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public string Gender { get; set; }
    public string Type { get; set; }
    public float WordsPerMinute { get; set; }
    public string[] Styles { get; set; }
    public string[]? Roles { get; set; }
}