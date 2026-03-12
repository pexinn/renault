using MsRenault.Dominio.DTOs.Renault;

namespace MsRenault.Dominio.Interfaces;

public interface IRenaultApiService
{
    Task<RenaultConsumeResponse> ConsumeLeadsAsync(string bir, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task UpdateFunnelAsync(string leadReferenceId, RenaultFunnelRequest request, CancellationToken ct = default);
    Task UpdateProspectionAsync(string leadReferenceId, RenaultProspectionRequest request, CancellationToken ct = default);
    Task UpdateSalesAsync(string leadReferenceId, RenaultSalesRequest request, CancellationToken ct = default);
    Task UpdateDeliveryAsync(string leadReferenceId, RenaultDeliveryRequest request, CancellationToken ct = default);
    Task<string> CreateLeadAsync(string bir, RenaultLeadData lead, CancellationToken ct = default);
}
