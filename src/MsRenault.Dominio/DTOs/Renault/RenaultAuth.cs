namespace MsRenault.Dominio.DTOs.Renault;

public record RenaultAuthRequest
{
    public string AccessKey { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Scope { get; init; } = "Crm";
}

public record RenaultAuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
    public string Message { get; init; } = string.Empty;
}
