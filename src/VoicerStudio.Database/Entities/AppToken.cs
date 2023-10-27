using NeerCore.Data.Abstractions;

namespace VoicerStudio.Database.Entities;

public class AppToken : ICreatableEntity
{
    public required string UserToken { get; init; }
    public required string JwtToken { get; init; }
    public Guid UserId { get; set; }
    public required DateTime Expired { get; init; }
    public DateTime Created { get; init; }


    public AppUser? User { get; init; }
}