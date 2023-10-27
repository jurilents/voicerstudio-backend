using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VoicerStudio.Database.Entities;
using VoicerStudio.TelegramBot.Bot.Core;

namespace VoicerStudio.TelegramBot.Bot.Commands;

internal class StartCommandHandler : CommandHandler
{
    public StartCommandHandler(IServiceProvider provider) : base(provider) { }

    public override string Command => "/start";


    public override async Task HandleAsync(Message message, CancellationToken ct)
    {
        if (message.Chat.Type != ChatType.Private) return;

        var user = await DbContext.Users.FirstOrDefaultAsync(x => x.TgUserId == message.From!.Id, ct);
        if (user is null)
        {
            user = new AppUser
            {
                TgUserId = message.From!.Id,
                TgUsername = message.From.Username,
                FullName = $"{message.From.FirstName} {message.From.LastName}".Trim(),
                Language = GetLanguageCode(message),
            };
            await DbContext.Users.AddAsync(user, ct);
            await DbContext.SaveChangesAsync(ct);

            message = await BotClient.SendTextMessageAsync(ChatId,
                text: GetMessagesForLanguage(user.Language).StartWelcome(),
                replyMarkup: RequestAccessKeyboard(),
                cancellationToken: ct);
        }
        else
        {
            message = await RespondByUserStatusAsync(user, null, ct);
        }

        Logger.LogInformation("The message {MessageId} was sent to the chat {ChatId}", message.MessageId, message.Chat.Id);
    }

    private static string GetLanguageCode(Message message)
    {
        var langCode = message.From!.LanguageCode;
        if (string.IsNullOrEmpty(langCode)) return "en";
        const string ruLanguages = "ru,be,uk,ky,ab,mo,os,tg,tt,uz,ce,az";
        return ruLanguages.Contains(langCode) ? "ru" : "en";
    }

    private static ReplyKeyboardMarkup RequestAccessKeyboard() => new(new[]
    {
        new[]
        {
            new KeyboardButton("Request an Access")
        }
    });
}