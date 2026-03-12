using MsRenault.Dominio.DTOs.Salesforce;

namespace MsRenault.Dominio.Interfaces;

public interface ISalesforceApiService
{
    Task<string> CreateLeadAsync(SalesforceLead lead, CancellationToken ct = default);
}
