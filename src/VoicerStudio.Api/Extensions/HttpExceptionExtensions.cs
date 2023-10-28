using FluentValidation;
using NeerCore.Exceptions;
using VoicerStudio.Application.Models;

namespace VoicerStudio.Api.Extensions;

public static class HttpExceptionExtensions
{
    public static Error CreateError(this HttpException e) => new(
        Code: (int)e.StatusCode,
        Message: e.Message);

    public static Error CreateFluentValidationError(this ValidationException e) => new(
        Code: 400,
        Message: "Invalid model received",
        Errors: e.Errors.ToDictionary(ve => ve.PropertyName, ve => ve.ErrorMessage)
    );
}