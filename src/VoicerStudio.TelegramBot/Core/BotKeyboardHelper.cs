using System.Text.Json;
using NeerCore.Json;
using Telegram.Bot.Types.ReplyMarkups;
using VoicerStudio.Database.Enums;
using VoicerStudio.TelegramBot.Models;

namespace VoicerStudio.TelegramBot.Core;

public static class BotKeyboardHelper
{
    public static InlineKeyboardMarkup AcceptRejectKeyboard(string command, StartCallbackPayload payload) => new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("✅", GetCommandString(command, payload with { Status = AccessStatus.Accepted })),
            InlineKeyboardButton.WithUrl("💬", $"tg://user?id={payload.UserId}"),
            InlineKeyboardButton.WithCallbackData("🚫", GetCommandString(command, payload with { Status = AccessStatus.Rejected })),
        }
    });

    public static InlineKeyboardMarkup UnsetKeyboard(string command, StartCallbackPayload payload) => new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("↩  Undo", GetCommandString(command, payload with { Status = AccessStatus.New })),
            InlineKeyboardButton.WithUrl("💬", $"tg://user?id={payload.UserId}"),
        }
    });


    private static string GetCommandString<T>(string command, T obj) =>
        command + "_" + JsonSerializer.Serialize(obj, JsonConventions.CamelCase);

    public static ReplyKeyboardMarkup LoginKeyboard() => new(new[]
    {
        new KeyboardButton[]
        {
            new("Login  🔑")
        }
    });
}