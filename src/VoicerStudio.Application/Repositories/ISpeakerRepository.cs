using VoicerStudio.Application.Models;

namespace VoicerStudio.Application.Repositories;

public interface ISpeakerRepository
{
    Task<Speaker> GetByIdAsync(int id, string sheet);
    Task<IEnumerable<Speaker>> GetAllAsync(string sheet);
    Task UpdateAsync(IEnumerable<Speaker> speakers, string sheet);
}