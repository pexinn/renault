using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MsRenault.Dominio.DTOs.Renault;
using MsRenault.Dominio.Interfaces;
using System.Diagnostics;

namespace MsRenault.Infra.Dados.Services;

public class RenaultAuthService : IRenaultAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RenaultAuthService> _logger;
    private readonly ActivitySource _activitySource;

    private string? _accessToken;
    private string? _secretKey;
    private DateTime _expiresAt;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RenaultAuthService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<RenaultAuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _activitySource = new ActivitySource("MsRenault.Auth");
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (NeedsRefresh())
        {
            await RefreshTokenAsync(ct);
        }
        return _accessToken ?? throw new InvalidOperationException("Failed to obtain access token.");
    }

    public async Task<string> GetSecretKeyAsync(CancellationToken ct = default)
    {
        if (NeedsRefresh())
        {
            await RefreshTokenAsync(ct);
        }
        return _secretKey ?? throw new InvalidOperationException("Failed to obtain secret key.");
    }

    private bool NeedsRefresh()
    {
        // Refresh 1 day before expiration if possible, or if null
        return string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _expiresAt.AddDays(-1);
    }

    private async Task RefreshTokenAsync(CancellationToken ct)
    {
        using var activity = _activitySource.StartActivity("Renault.RefreshToken");
        await _lock.WaitAsync(ct);
        try
        {
            // Double check inside lock
            if (!NeedsRefresh()) return;

            _logger.LogInformation("Refreshing Renault access token...");

            var request = new RenaultAuthRequest
            {
                AccessKey = _configuration["Renault:AccessKey"] ?? string.Empty,
                Password = _configuration["Renault:Password"] ?? string.Empty,
                Scope = "Crm"
            };

            var baseUrl = _configuration["Renault:BaseUrl"] ?? "https://crmi.renault.com.br";
            var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/v1/auth/client", request, ct);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<RenaultAuthResponse>(cancellationToken: ct);
                if (content != null && !string.IsNullOrEmpty(content.AccessToken))
                {
                    _accessToken = content.AccessToken;
                    _secretKey = content.SecretKey;
                    // According to requirements, token lasts 90 days. 
                    // If API doesn't return exactly when, we set it to 90 days as per documentation.
                    _expiresAt = DateTime.UtcNow.AddDays(90); 
                    
                    _logger.LogInformation("Renault token refreshed successfully. Expires at {ExpiresAt}", _expiresAt);
                    return;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to authenticate with Renault API. Status: {Status}, Error: {Error}", response.StatusCode, errorContent);
            activity?.SetStatus(ActivityStatusCode.Error, "Authentication failed");
            
            throw new HttpRequestException($"Renault Authentication failed: {response.StatusCode} - {errorContent}");
        }
        finally
        {
            _lock.Release();
        }
    }
}
