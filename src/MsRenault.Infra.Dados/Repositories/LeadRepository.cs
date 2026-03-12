using MongoDB.Driver;
using MsRenault.Dominio.Entities;
using MsRenault.Dominio.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MsRenault.Infra.Dados.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly IMongoCollection<RawRenaultLead> _leads;

    public LeadRepository(IMongoClient mongoClient, IConfiguration configuration)
    {
        var databaseName = configuration["MongoDB:Database"] ?? "MsRenaultDB";
        var database = mongoClient.GetDatabase(databaseName);
        _leads = database.GetCollection<RawRenaultLead>("RawLeads");
    }

    public async Task CreateAsync(RawRenaultLead lead, CancellationToken ct = default)
    {
        await _leads.InsertOneAsync(lead, cancellationToken: ct);
    }

    public async Task UpdateStatusAsync(string leadReferenceId, string status, string? salesforceId = null, CancellationToken ct = default)
    {
        var update = Builders<RawRenaultLead>.Update
            .Set(l => l.Status, status);

        if (!string.IsNullOrEmpty(salesforceId))
        {
            update = update.Set(l => l.SalesforceLeadId, salesforceId);
        }

        await _leads.UpdateOneAsync(l => l.LeadReferenceId == leadReferenceId, update, cancellationToken: ct);
    }

    public async Task<RawRenaultLead?> GetByLeadReferenceIdAsync(string leadReferenceId, CancellationToken ct = default)
    {
        return await _leads.Find(l => l.LeadReferenceId == leadReferenceId).FirstOrDefaultAsync(ct);
    }
}
