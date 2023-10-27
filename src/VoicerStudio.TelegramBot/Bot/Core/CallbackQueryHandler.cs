using Telegram.Bot.Types;

namespace VoicerStudio.TelegramBot.Bot.Core;

public abstract class CallbackQueryHandler : TelegramHandlerBase
{
    protected CallbackQueryHandler(IServiceProvider provider) : base(provider) { }

    public abstract Task HandleAsync(CallbackQuery callbackQuery, CancellationToken ct);
}