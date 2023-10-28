using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VoicerStudio.Database.Repositories;
using VoicerStudio.TelegramBot.Core;

namespace VoicerStudio.TelegramBot.Commands;

internal class LoginCommandHandler : CommandHandler
{
    private readonly ITokenRepository _tokenRepository;

    public LoginCommandHandler(IServiceProvider provider, ITokenRepository tokenRepository) : base(provider)
    {
        _tokenRepository = tokenRepository;
    }

    public override string Command => "/login";


    public override async Task HandleAsync(Message message, CancellationToken ct)
    {
        if (message.Chat.Type != ChatType.Private) return;

        var user = await DbContext.Users.FirstOrDefaultAsync(x => x.TgUserId == message.From!.Id, ct);

        if (user is null)
        {
            await SendUserNotExistsMessageAsync(ct);
            return;
        }

        var token = await _tokenRepository.GenerateAsync(user.Id, ct);

        await BotClient.SendTextMessageAsync(ChatId,
            text: GetMessagesForLanguage(user.Language).LoginTokenInfo(token.UserToken, message.Date),
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }
}