namespace MsRenault.Dominio.DTOs.Salesforce;

public record SalesforceLead
{
    public string? Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? MobilePhone { get; init; }
    public string? Email { get; init; }
    public string? LeadSource { get; init; }
    public string? Status { get; init; }
    public string? ModelOfInterest { get; init; }
    public string? RenaultLeadReferenceId { get; init; }
    
    // Phase 3 fields
    public string? TypeOfInterest { get; init; }
    public string? SubTypeOfInterest { get; init; }
    public string? Origin { get; init; }
    public string? Dealer { get; init; }
    public DateTime? CreatedDate { get; init; }
}
