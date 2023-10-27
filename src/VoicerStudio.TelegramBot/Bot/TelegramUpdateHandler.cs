using NeerCore.DependencyInjection;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using VoicerStudio.TelegramBot.Bot.Core;

namespace VoicerStudio.TelegramBot.Bot;

[Service(Lifetime = Lifetime.Scoped)]
public class TelegramUpdateHandler
{
    private readonly IList<CommandHandler> _commandHandlers;
    private readonly IList<CallbackQueryHandler> _callbackQueryHandlers;
    private readonly ILogger<TelegramUpdateHandler> _logger;

    public TelegramUpdateHandler(
        IEnumerable<CommandHandler> commandHandlers,
        IEnumerable<CallbackQueryHandler> callbackQueryHandlers,
        ILogger<TelegramUpdateHandler> logger)
    {
        _commandHandlers = commandHandlers.ToList();
        _callbackQueryHandlers = callbackQueryHandlers.ToList();
        _logger = logger;
    }

    public Task HandleErrorAsync(Exception exception, CancellationToken ct)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("Handle telegram bot error: {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            { Message: { } message }             => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message }       => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            // { InlineQuery: { } inlineQuery }               => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
            // { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
            // _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken ct)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        messageText = messageText switch
        {
            "Request an Access" => "/request_access",
            "/lang_eng"         => "/lang en",
            "/lang_rus"         => "/lang ru",
            _                   => messageText
        };
        if (messageText.StartsWith("Login")) messageText = "/login";
        message.Text = messageText;
        var commandName = messageText.Split(' ')[0].Replace("@voicerstudio_bot", "").ToLower();

        var handler = _commandHandlers.FirstOrDefault(x => x.Command == commandName);
        if (handler is null) return;
        handler.ChatId = message.Chat.Id;
        await handler.HandleAsync(message, ct);
    }

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken ct)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        var commandName = string.IsNullOrEmpty(callbackQuery.Data)
            ? null
            : callbackQuery.Data[..(callbackQuery.Data.IndexOf('{') - 1)].ToLower();
        if (string.IsNullOrEmpty(commandName)) return;

        var handler = _callbackQueryHandlers.FirstOrDefault(x => x.Command == commandName);
        if (handler is null) return;
        handler.ChatId = callbackQuery.Message!.Chat.Id;
        await handler.HandleAsync(callbackQuery, ct);
    }


    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken ct)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}