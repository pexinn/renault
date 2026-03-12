using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using MsRenault.Dominio.DTOs.Renault;
using MsRenault.Dominio.Interfaces;

namespace MsRenault.Infra.Dados.Services;

public class RenaultApiService : IRenaultApiService
{
    private readonly HttpClient _httpClient;
    private readonly IRenaultAuthService _authService;
    private readonly IConfiguration _configuration;

    public RenaultApiService(HttpClient httpClient, IRenaultAuthService authService, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _authService = authService;
        _configuration = configuration;
    }

    public async Task<RenaultConsumeResponse> ConsumeLeadsAsync(string bir, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var token = await _authService.GetAccessTokenAsync(ct);
        var secretKey = await _authService.GetSecretKeyAsync(ct);

        var request = new RenaultConsumeRequest
        {
            Bir = bir,
            StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        var baseUrl = GetBaseUrl();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/leads/consume");
        AddAuthHeaders(httpRequest, token, secretKey);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RenaultConsumeResponse>(cancellationToken: ct) 
               ?? new RenaultConsumeResponse();
    }

    public async Task UpdateFunnelAsync(string leadReferenceId, RenaultFunnelRequest request, CancellationToken ct = default)
    {
        await SendFeedbackAsync($"v1/leads/complete/{leadReferenceId}/funnel", request, ct);
    }

    public async Task UpdateProspectionAsync(string leadReferenceId, RenaultProspectionRequest request, CancellationToken ct = default)
    {
        await SendFeedbackAsync($"v1/leads/complete/{leadReferenceId}/prospection", request, ct);
    }

    public async Task UpdateSalesAsync(string leadReferenceId, RenaultSalesRequest request, CancellationToken ct = default)
    {
        await SendFeedbackAsync($"v1/leads/complete/{leadReferenceId}/sales", request, ct);
    }

    public async Task UpdateDeliveryAsync(string leadReferenceId, RenaultDeliveryRequest request, CancellationToken ct = default)
    {
        await SendFeedbackAsync($"v1/leads/complete/{leadReferenceId}/delivery", request, ct);
    }

    public async Task<string> CreateLeadAsync(string bir, RenaultLeadData lead, CancellationToken ct = default)
    {
        var token = await _authService.GetAccessTokenAsync(ct);
        var secretKey = await _authService.GetSecretKeyAsync(ct);

        var baseUrl = GetBaseUrl();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/leads/create/{bir}");
        AddAuthHeaders(httpRequest, token, secretKey);
        httpRequest.Content = JsonContent.Create(lead);

        var response = await _httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        // Assuming the response contains leadReferenceId
        var result = await response.Content.ReadFromJsonAsync<RenaultCreateResponse>(cancellationToken: ct);
        return result?.LeadReferenceId ?? string.Empty;
    }

    private async Task SendFeedbackAsync<T>(string relativeUrl, T payload, CancellationToken ct)
    {
        var token = await _authService.GetAccessTokenAsync(ct);
        var secretKey = await _authService.GetSecretKeyAsync(ct);

        var baseUrl = GetBaseUrl();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/{relativeUrl}");
        AddAuthHeaders(httpRequest, token, secretKey);
        httpRequest.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();
    }

    private string GetBaseUrl() => _configuration["Renault:BaseUrl"] ?? "https://crmi.renault.com.br";

    private void AddAuthHeaders(HttpRequestMessage request, string token, string secretKey)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("SecretKey", secretKey);
    }

    private record RenaultCreateResponse(string LeadReferenceId);
}
