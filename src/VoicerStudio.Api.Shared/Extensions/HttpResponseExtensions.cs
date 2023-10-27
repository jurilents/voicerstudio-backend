using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NeerCore.Exceptions;
using NeerCore.Json;

namespace VoicerStudio.Api.Shared.Extensions;

public static class HttpResponseExtensions
{
    public static async Task WriteJsonAsync<TResponse>(this HttpResponse response, HttpStatusCode statusCode, TResponse model)
    {
        response.ContentType = "application/json";
        response.StatusCode = (int)statusCode;
        await response.WriteAsync(JsonSerializer.Serialize(model, JsonConventions.CamelCase));
    }

    public static async Task Write500ErrorAsync(this HttpResponse response, Exception exception, bool extended = false)
    {
        var message = extended ? exception.Message : "Server Error";
        var error = new InternalServerException(message, exception).CreateError();

        await response.WriteJsonAsync(HttpStatusCode.InternalServerError, error);
    }

    public static void AddDurationHeader(this HttpResponse response, TimeSpan outputDuration, TimeSpan? inputDuration = null)
    {
        if (inputDuration.HasValue)
        {
            var baseDuration = (inputDuration.Value.TotalMilliseconds / 1000.0).ToString(CultureInfo.InvariantCulture);
            response.Headers.Add("X-Base-Duration", baseDuration);
        }

        var duration = (outputDuration.TotalMilliseconds / 1000.0).ToString(CultureInfo.InvariantCulture);
        response.Headers.Add("X-Duration", duration);
        response.Headers.Add("Access-Control-Expose-Headers", "X-Base-Duration,X-Duration");
    }
}