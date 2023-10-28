using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using VoicerStudio.Database.Context;

namespace VoicerStudio.TelegramBot.HostedServices;

public class DeleteExpiredTokensHostedService : IHostedService
{
    private Timer? _timer;

    private readonly AppDbContext _dbContext;

    public DeleteExpiredTokensHostedService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public Task StartAsync(CancellationToken ct)
    {
        _timer = new Timer(Work, ct, TimeSpan.MaxValue, TimeSpan.FromHours(6));
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct)
    {
        if (_timer is not null)
            await _timer.DisposeAsync();
    }


    private async void Work(object? state)
    {
        var ct = (CancellationToken)state!;
        var expirationLimit = DateTime.UtcNow.AddDays(-2);
        var expiredTokens = await _dbContext.Tokens.Where(x => x.Expired < expirationLimit).ToArrayAsync(ct);
        _dbContext.Tokens.RemoveRange(expiredTokens);
        await _dbContext.SaveChangesAsync(ct);
    }
}