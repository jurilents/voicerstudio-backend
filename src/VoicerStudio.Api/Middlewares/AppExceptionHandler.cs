using System.Net;
using FluentValidation;
using NeerCore.Exceptions;
using VoicerStudio.Api.Extensions;

namespace VoicerStudio.Api.Middlewares;

internal sealed class AppExceptionHandler : IMiddleware
{
    private readonly ILogger<AppExceptionHandler> _logger;

    public AppExceptionHandler(ILogger<AppExceptionHandler> logger)
    {
        _logger = logger;
    }


    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await context.Response.WriteJsonAsync(HttpStatusCode.BadRequest, ex.CreateFluentValidationError());
            // await ProcessCommonExceptionAsync(context, ex);
        }
        catch (HttpException ex)
        {
            if (ex.StatusCode >= HttpStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Internal Server Error");
                await context.Response.Write500ErrorAsync(ex, true);
            }
            else
            {
                await context.Response.WriteJsonAsync(ex.StatusCode, ex.CreateError());
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Request {Path} was canceled", context.Request.Path.Value);
        }
        catch (Exception ex)
        {
            await ProcessCommonExceptionAsync(context, ex);
        }
    }

    private async Task ProcessCommonExceptionAsync(HttpContext context, Exception e)
    {
        _logger.LogError(e, "Unhandled Server Error");
        await context.Response.Write500ErrorAsync(e, true);
    }
}