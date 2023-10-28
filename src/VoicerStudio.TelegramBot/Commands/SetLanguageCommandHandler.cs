using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VoicerStudio.TelegramBot.Core;

namespace VoicerStudio.TelegramBot.Commands;

internal class SetLanguageCommandHandler : CommandHandler
{
    public SetLanguageCommandHandler(IServiceProvider provider) : base(provider) { }

    public override string Command => "/lang";

    private static readonly string[] allowedLanguages = { "en", "ru" };


    public override async Task HandleAsync(Message message, CancellationToken ct)
    {
        if (message.Chat.Type != ChatType.Private) return;

        var user = await DbContext.Users.FirstOrDefaultAsync(x => x.TgUserId == message.From!.Id, ct);

        if (user is null)
        {
            await SendUserNotExistsMessageAsync(ct);
            return;
        }

        var msgParts = message.Text?.Split(' ');
        user.Language = msgParts?.Length > 1 ? msgParts[1].ToLowerInvariant() : "en";
        if (!allowedLanguages.Contains(user.Language)) user.Language = "en";

        await BotClient.SendTextMessageAsync(ChatId,
            text: GetMessagesForLanguage(user.Language).SetLanguage(),
            cancellationToken: ct);

        await DbContext.SaveChangesAsync(ct);
    }
}