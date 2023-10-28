using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using VoicerStudio.Application.Options;
using VoicerStudio.Database.Context;
using VoicerStudio.Database.Entities;
using VoicerStudio.Database.Enums;
using VoicerStudio.TelegramBot.Localization;

namespace VoicerStudio.TelegramBot.Core;

public abstract class TelegramHandlerBase
{
    private ILogger? _logger;
    private AppDbContext? _dbContext;
    private TelegramOptions? _telegram;

    private readonly IServiceScope _serviceScope;
    protected readonly ITelegramBotClient BotClient;
    private readonly LocalizedMessagesManager _messagesManager;

    protected IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;
    protected AppDbContext DbContext => _dbContext ??= ServiceProvider.GetRequiredService<AppDbContext>();
    protected TelegramOptions Telegram => _telegram ??= ServiceProvider.GetRequiredService<IOptions<TelegramOptions>>().Value;
    protected ILogger Logger => _logger ??= ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

    protected TelegramHandlerBase(IServiceProvider provider)
    {
        _serviceScope = provider.CreateScope();
        BotClient = provider.GetRequiredService<ITelegramBotClient>();
        _messagesManager = provider.GetRequiredService<LocalizedMessagesManager>();
    }

    public ChatId ChatId { get; set; } = null!;


    public abstract string Command { get; }

    protected ILocalizedMessages GetMessagesForLanguage(string langCode)
    {
        return _messagesManager.FromLanguage(langCode);
    }

    protected async Task SendUserNotExistsMessageAsync(CancellationToken ct = default)
    {
        await BotClient.SendTextMessageAsync(ChatId,
            text: "Type /start to start using this bot.",
            cancellationToken: ct);
    }


    protected async Task<Message> RespondByUserStatusAsync(AppUser user, AppUser? admin = null, CancellationToken ct = default)
    {
        return user.Status switch
        {
            AccessStatus.New or AccessStatus.Pending => await BotClient.SendTextMessageAsync(ChatId,
                text: GetMessagesForLanguage(user.Language).CommonResponseForStatusNew(),
                cancellationToken: ct),
            AccessStatus.Accepted => await BotClient.SendTextMessageAsync(ChatId,
                text: GetMessagesForLanguage(user.Language).CommonResponseForStatusAccepted(),
                cancellationToken: ct),
            AccessStatus.Rejected => await BotClient.SendTextMessageAsync(ChatId,
                text: GetMessagesForLanguage(user.Language).CommonResponseForStatusRejected(admin?.TgUsername),
                cancellationToken: ct),
            _ => throw new ArgumentOutOfRangeException($"User status is not supported: '{user.Status}'")
        };
    }
}