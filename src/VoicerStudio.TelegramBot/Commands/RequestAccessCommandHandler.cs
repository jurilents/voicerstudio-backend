using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VoicerStudio.Database.Enums;
using VoicerStudio.TelegramBot.Core;
using VoicerStudio.TelegramBot.Models;

namespace VoicerStudio.TelegramBot.Commands;

internal class RequestAccessCommandHandler : CommandHandler
{
    public RequestAccessCommandHandler(IServiceProvider provider) : base(provider) { }

    public override string Command => "/request_access";


    public override async Task HandleAsync(Message message, CancellationToken ct)
    {
        if (message.Chat.Type != ChatType.Private) return;

        var user = await DbContext.Users
            .Include(x => x.AdminWhoSetStatus)
            .FirstOrDefaultAsync(x => x.TgUserId == message.From!.Id, ct);

        if (user is null)
        {
            await SendUserNotExistsMessageAsync(ct);
            return;
        }

        var notificationText = user.Status switch
        {
            AccessStatus.New      => GetMessagesForLanguage(user.Language).AccessRequestSent(),
            AccessStatus.Pending  => GetMessagesForLanguage(user.Language).AccessRequestIsPending(),
            AccessStatus.Accepted => GetMessagesForLanguage(user.Language).AccessRequestAlreadyAccepted(),
            AccessStatus.Rejected => GetMessagesForLanguage(user.Language).AccessRequestAlreadyRejected(user.AdminWhoSetStatus?.TgUsername),
            _                     => throw new ArgumentOutOfRangeException(nameof(user.Status), "Unknown user status")
        };

        if (user.Status is AccessStatus.New)
        {
            await BotClient.SendTextMessageAsync(Telegram.AdminChatId,
                text: RequestAccessTextHelper.GetText(user, message.Date, null, AccessStatus.New),
                parseMode: ParseMode.Html,
                replyMarkup: BotKeyboardHelper.AcceptRejectKeyboard(Command, StartCallbackPayload.FromUser(user)),
                cancellationToken: ct);

            user.Status = AccessStatus.Pending;
            await DbContext.SaveChangesAsync(ct);
        }

        await BotClient.SendTextMessageAsync(ChatId,
            text: notificationText,
            parseMode: ParseMode.Html,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: ct);
    }
}