using System.Text.Json.Serialization;

namespace MsRenault.Dominio.DTOs.Renault;

public record RenaultConsumeRequest
{
    [JsonPropertyName("bir")]
    public string Bir { get; init; } = string.Empty;

    [JsonPropertyName("startDate")]
    public string StartDate { get; init; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; init; } = string.Empty;
}

public record RenaultConsumeResponse
{
    [JsonPropertyName("data")]
    public List<RenaultLeadData> Data { get; init; } = new();

    [JsonPropertyName("total")]
    public int Total { get; init; }
}

public record RenaultLeadData
{
    [JsonPropertyName("leadReferenceId")]
    public string LeadReferenceId { get; init; } = string.Empty;

    [JsonPropertyName("client")]
    public RenaultClient Client { get; init; } = new();

    [JsonPropertyName("vehicle")]
    public RenaultVehicle Vehicle { get; init; } = new();

    [JsonPropertyName("submissionTimestamp")]
    public string SubmissionTimestamp { get; init; } = string.Empty;
}

public record RenaultClient
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; init; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; init; } = string.Empty;

    [JsonPropertyName("mobilePhone")]
    public string MobilePhone { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;
}

public record RenaultVehicle
{
    [JsonPropertyName("modelOfInterest")]
    public string ModelOfInterest { get; init; } = string.Empty;
}
