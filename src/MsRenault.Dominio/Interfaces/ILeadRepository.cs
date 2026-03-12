using MsRenault.Dominio.Entities;

namespace MsRenault.Dominio.Interfaces;

public interface ILeadRepository
{
    Task CreateAsync(RawRenaultLead lead, CancellationToken ct = default);
    Task UpdateStatusAsync(string leadReferenceId, string status, string? salesforceId = null, CancellationToken ct = default);
    Task<RawRenaultLead?> GetByLeadReferenceIdAsync(string leadReferenceId, CancellationToken ct = default);
}
