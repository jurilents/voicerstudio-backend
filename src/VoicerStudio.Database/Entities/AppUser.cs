using NeerCore.Data.Abstractions;
using VoicerStudio.Database.Enums;

namespace VoicerStudio.Database.Entities;

public class AppUser : IDateableEntity<Guid>
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string? FullName { get; init; }
    public required long TgUserId { get; init; }
    public string? TgUsername { get; init; }
    public string Language { get; set; } = "en";
    public UserRole Role { get; set; }
    public AccessStatus Status { get; set; }
    public Guid? AdminWhoSetStatusId { get; set; }
    public DateTime? Updated { get; init; }
    public DateTime Created { get; init; }


    public AppUser? AdminWhoSetStatus { get; set; }
    public ICollection<AppToken>? Tokens { get; set; }
}