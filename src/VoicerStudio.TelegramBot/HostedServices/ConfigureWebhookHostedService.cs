using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using VoicerStudio.Application.Options;

namespace VoicerStudio.TelegramBot.HostedServices;

public class ConfigureWebhookHostedService : IHostedService
{
    private readonly ILogger<ConfigureWebhookHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TelegramOptions _options;

    public ConfigureWebhookHostedService(
        ILogger<ConfigureWebhookHostedService> logger,
        IServiceProvider serviceProvider,
        IOptions<TelegramOptions> optionsAccessor)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = optionsAccessor.Value;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Configure custom endpoint per Telegram API recommendations:
        // https://core.telegram.org/bots/api#setwebhook
        // If you'd like to make sure that the webhook was set by you, you can specify secret data
        // in the parameter secret_token. If specified, the request will contain a header
        // "X-Telegram-Bot-Api-Secret-Token" with the secret token as content.
        _logger.LogInformation("Setting webhook: {WebhookAddress}", _options.WebhookUrl);
        await botClient.SetWebhookAsync(
            url: _options.WebhookUrl,
            allowedUpdates: Array.Empty<UpdateType>(),
            secretToken: _options.SecretToken,
            cancellationToken: ct);
    }

    public async Task StopAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Remove webhook on app shutdown
        _logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: ct);
    }
}