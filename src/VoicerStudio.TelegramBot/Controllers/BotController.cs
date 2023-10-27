using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using VoicerStudio.TelegramBot.Bot;
using VoicerStudio.TelegramBot.Filters;

namespace VoicerStudio.TelegramBot.Controllers;

[ApiController]
public class BotController : ControllerBase
{
    private readonly TelegramUpdateHandler _updateHandler;

    public BotController(TelegramUpdateHandler updateHandler)
    {
        _updateHandler = updateHandler;
    }


    [ValidateTelegramBot]
    [HttpPost("/webhook")]
    public async Task<IActionResult> Post([FromBody] Update update, CancellationToken ct)
    {
        await _updateHandler.HandleUpdateAsync(update, ct);
        return Ok();
    }
}