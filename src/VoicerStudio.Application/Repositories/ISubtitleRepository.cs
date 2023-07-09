using VoicerStudio.Application.Models;

namespace VoicerStudio.Application.Repositories;

public interface ISubtitleRepository
{
    Task<Subtitle> GetByIdAsync(Guid id, string sheet);
    Task<IEnumerable<Subtitle>> GetAllAsync(string sheet);
    Task UpdateAsync(IEnumerable<Subtitle> subs, string sheet);
}