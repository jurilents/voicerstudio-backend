namespace VoicerStudio.Application.Repositories;

public interface IEventRepository
{
    Task<object> GetTimingAsync(string slug, CancellationToken ct = default);
    Task SetTimingAsync(string slug, object timing, CancellationToken ct = default);
}