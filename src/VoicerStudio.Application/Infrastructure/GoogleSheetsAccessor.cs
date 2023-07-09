// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeerCore.Exceptions;
using VoicerStudio.Application.Options;

namespace VoicerStudio.Application.Infrastructure;

internal sealed class GoogleSheetsAccessor : IDisposable
{
    private static readonly string[] scopes = { SheetsService.Scope.Spreadsheets };

    private readonly ILogger<GoogleSheetsAccessor> _logger;
    private readonly GoogleOptions _options;

    public SheetsService Service { get; private set; }

    public GoogleSheetsAccessor(ILogger<GoogleSheetsAccessor> logger, IOptions<GoogleOptions> optionsAccessor)
    {
        _logger = logger;
        _options = optionsAccessor.Value;
        var credential = GetCredentialsFromFile();
        Service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName,
        });
    }

    public async Task WrapRequestAsync(string sheet, Func<Task> callback)
    {
        if (sheet is null)
            throw new ValidationFailedException("Google Spreadsheet must be specified ('sheet')");
        try
        {
            await callback();
        }
        catch (GoogleApiException ex)
        {
            _logger.LogWarning("Google Spreadsheet API Error: {Msg}", ex.Message);
            throw new ValidationFailedException($"Google Spreadsheet Error: {ex.Message}");
        }
    }

    public async Task<T> WrapRequestAsync<T>(string sheet, Func<Task<T>> callback)
    {
        if (sheet is null)
            throw new ValidationFailedException("Google Spreadsheet must be specified ('sheet')");
        try
        {
            return await callback();
        }
        catch (GoogleApiException ex)
        {
            _logger.LogWarning("Google Spreadsheet API Error: {Msg}", ex.Message);
            throw new ValidationFailedException($"Google Spreadsheet Error: {ex.Message}");
        }
    }


    private GoogleCredential GetCredentialsFromFile()
    {
        using var stream = new FileStream(_options.SecretsPath, FileMode.Open, FileAccess.Read);
        var credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
        return credential;
    }


    public void Dispose()
    {
        Service.Dispose();
        GC.SuppressFinalize(this);
    }
}