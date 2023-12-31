using Telegram.Bot.Types;

namespace VoicerStudio.TelegramBot.Core;

public abstract class CommandHandler : TelegramHandlerBase
{
    protected CommandHandler(IServiceProvider provider) : base(provider) { }

    public abstract Task HandleAsync(Message message, CancellationToken ct);
}