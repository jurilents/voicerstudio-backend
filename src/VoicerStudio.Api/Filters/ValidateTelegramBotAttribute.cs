using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using VoicerStudio.Application.Options;

namespace VoicerStudio.Api.Filters;

/// <summary>
/// Check for "X-Telegram-Bot-Api-Secret-Token"
/// Read more: <see href="https://core.telegram.org/bots/api#setwebhook"/> "secret_token"
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ValidateTelegramBotAttribute : TypeFilterAttribute
{
    public ValidateTelegramBotAttribute() : base(typeof(ValidateTelegramBotFilter)) { }

    private class ValidateTelegramBotFilter : IActionFilter
    {
        private const string SecretHeaderName = "X-Telegram-Bot-Api-Secret-Token";

        private readonly string _secretToken;

        public ValidateTelegramBotFilter(IOptions<TelegramOptions> optionsAccessor)
        {
            var options = optionsAccessor.Value;
            _secretToken = options.SecretToken;
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsValidRequest(context.HttpContext.Request))
            {
                context.Result = new ObjectResult($"\"{SecretHeaderName}\" is invalid")
                {
                    StatusCode = 403
                };
            }
        }

        private bool IsValidRequest(HttpRequest request)
        {
            var isSecretTokenProvided = request.Headers.TryGetValue(SecretHeaderName, out var secretTokenHeader);
            if (!isSecretTokenProvided) return false;

            return string.Equals(secretTokenHeader, _secretToken, StringComparison.Ordinal);
        }
    }
}