using MsRenault.Dominio.DTOs.Renault;

namespace MsRenault.Dominio.Interfaces;

public interface IRenaultAuthService
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
    Task<string> GetSecretKeyAsync(CancellationToken ct = default);
}
