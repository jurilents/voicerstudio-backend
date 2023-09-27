using VoicerStudio.Application.Models.ConferenceTranslations;

namespace VoicerStudio.Application.Services;

public interface IConferenceTranslationsService
{
    Task<EventModel[]> GetEventsAsync(string authKey, CancellationToken ct = default);
}