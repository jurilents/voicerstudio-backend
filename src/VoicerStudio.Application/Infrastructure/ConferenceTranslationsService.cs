using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;
using Microsoft.Extensions.Caching.Memory;
using NeerCore.Exceptions;
using NeerCore.Json;
using VoicerStudio.Application.Models.ConferenceTranslations;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.Infrastructure;

internal class ConferenceTranslationsService : IConferenceTranslationsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;

    public ConferenceTranslationsService(HttpClient httpClient, IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
    }


    public async Task<EventModel[]> GetEventsAsync(string authKey, CancellationToken ct = default)
    {
        try
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.PreAuthenticate = true;
            httpClientHandler.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            using var httpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri("https://tr.creativesociety.com/api/v1") };
            // httpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue()
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Host", "tr.creativesociety.com");
            // AddAuthorizationHeader(authKey);
            var response = await httpClient.GetAsync("/events", ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CtArrayResult<CtEventDto>>(JsonConventions.CamelCase, ct);
            if (result?.Results is not null)
                return result.Results.Select(EventModel.FromCtDto).ToArray();
        }
        catch (HttpRequestException e)
        {
            if (e.InnerException?.GetType() == typeof(AuthenticationException))
                throw new UnauthorizedException("Username or password is not valid");
        }

        throw new InternalServerException("You authorized, but response from conference translations server is invalid");
    }


    private void AddAuthorizationHeader(string authKey)
    {
        if (string.IsNullOrEmpty(authKey)) return;
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {authKey}");
    }
}