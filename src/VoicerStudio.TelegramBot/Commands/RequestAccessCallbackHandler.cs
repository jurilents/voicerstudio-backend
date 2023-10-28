using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NeerCore.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VoicerStudio.Database.Enums;
using VoicerStudio.TelegramBot.Core;
using VoicerStudio.TelegramBot.Models;

namespace VoicerStudio.TelegramBot.Commands;

public class RequestAccessCallbackHandler : CallbackQueryHandler
{
    public RequestAccessCallbackHandler(IServiceProvider provider) : base(provider) { }

    public override string Command => "/request_access";


    public override async Task HandleAsync(CallbackQuery callbackQuery, CancellationToken ct)
    {
        var payload = ParseCallbackQueryPayload(callbackQuery);

        var user = await DbContext.Users.FirstOrDefaultAsync(x => x.TgUserId == payload.UserId, ct);
        var admin = await DbContext.Users.FirstOrDefaultAsync(x => x.TgUserId == callbackQuery.From.Id, ct);
        if (admin?.Role is not UserRole.Admin) return;

        if (user is null)
        {
            await SendUserNotExistsMessageAsync(ct);
            return;
        }

        user.Status = payload.Status;
        user.AdminWhoSetStatusId = admin.Id;
        await DbContext.SaveChangesAsync(ct);

        await BotClient.EditMessageTextAsync(Telegram.AdminChatId,
            messageId: callbackQuery.Message!.MessageId,
            text: RequestAccessTextHelper.GetText(user, callbackQuery.Message.Date, admin, payload.Status),
            parseMode: ParseMode.Html,
            replyMarkup: user.Status is not AccessStatus.New
                ? BotKeyboardHelper.UnsetKeyboard(Command, payload)
                : BotKeyboardHelper.AcceptRejectKeyboard(Command, payload),
            cancellationToken: ct);

        var notificationText = user.Status switch
        {
            AccessStatus.Accepted => GetMessagesForLanguage(user.Language).NotifyAccessAccepted(),
            AccessStatus.Rejected => GetMessagesForLanguage(user.Language).NotifyAccessRejected(admin.TgUsername),
            _                     => null
        };

        if (notificationText is null) return;
        await BotClient.SendTextMessageAsync(user.TgUserId,
            text: notificationText,
            parseMode: ParseMode.Html,
            replyMarkup: user.Status is AccessStatus.Accepted ? BotKeyboardHelper.LoginKeyboard() : null,
            cancellationToken: ct);
    }


    private static StartCallbackPayload ParseCallbackQueryPayload(CallbackQuery callbackQuery)
    {
        var str = callbackQuery.Data.AsSpan()[callbackQuery.Data!.IndexOf('{')..];
        return JsonSerializer.Deserialize<StartCallbackPayload>(str, JsonConventions.CamelCase)!;
    }
}