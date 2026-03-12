using System.Text.Json.Serialization;

namespace MsRenault.Dominio.DTOs.Renault;

public record RenaultFunnelRequest
{
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("statusUpdatedAt")]
    public string StatusUpdatedAt { get; init; } = string.Empty;
}

public record RenaultProspectionRequest
{
    [JsonPropertyName("requestDateUpdate")]
    public string RequestDateUpdate { get; init; } = string.Empty;

    [JsonPropertyName("prospectionLeadCreationDate")]
    public string ProspectionLeadCreationDate { get; init; } = string.Empty;

    [JsonPropertyName("prospectionAttendantName")]
    public string ProspectionAttendantName { get; init; } = string.Empty;

    [JsonPropertyName("prospectionSalesName")]
    public string ProspectionSalesName { get; init; } = string.Empty;

    [JsonPropertyName("prospectionContactSuccess")]
    public string ProspectionContactSuccess { get; init; } = string.Empty;

    [JsonPropertyName("prospectionTestDriveDate")]
    public string? ProspectionTestDriveDate { get; init; }
}

public record RenaultSalesRequest
{
    [JsonPropertyName("salesUsedCarEnchange")]
    public bool SalesUsedCarEnchange { get; init; }

    [JsonPropertyName("salesPaymentMethod")]
    public string SalesPaymentMethod { get; init; } = string.Empty;

    [JsonPropertyName("salesProposalValue1")]
    public decimal? SalesProposalValue1 { get; init; }
}

public record RenaultDeliveryRequest
{
    [JsonPropertyName("deliveryChassisNumber")]
    public string? DeliveryChassisNumber { get; init; }

    [JsonPropertyName("deliveryExpectedDeliveryDate")]
    public string? DeliveryExpectedDeliveryDate { get; init; }
}
