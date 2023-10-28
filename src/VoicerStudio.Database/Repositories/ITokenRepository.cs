using VoicerStudio.Database.Entities;

namespace VoicerStudio.Database.Repositories;

public interface ITokenRepository
{
    Task<AppToken> GenerateAsync(Guid userId, CancellationToken ct = default);
}