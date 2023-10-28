using VoicerStudio.Database.Entities;

namespace VoicerStudio.Application.Repositories;

public interface ITokenRepository
{
    Task<AppToken> GenerateAsync(Guid userId, CancellationToken ct = default);
}