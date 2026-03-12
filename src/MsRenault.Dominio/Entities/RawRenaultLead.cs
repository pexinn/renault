using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MsRenault.Dominio.Entities;

public class RawRenaultLead
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string LeadReferenceId { get; set; } = string.Empty;
    public string Bir { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string RawJson { get; set; } = string.Empty;
    
    public string? SalesforceLeadId { get; set; }
    public string Status { get; set; } = "Received"; // Received, SentToSalesforce, Failed
}
