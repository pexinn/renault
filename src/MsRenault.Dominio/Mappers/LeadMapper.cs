using System.Globalization;
using MsRenault.Dominio.DTOs.Renault;
using MsRenault.Dominio.DTOs.Salesforce;
using MsRenault.Dominio.Constants;

namespace MsRenault.Dominio.Mappers;

public static class LeadMapper
{
    private const string UtcFormat = "yyyy-MM-ddTHH:mm:ssZ";

    public static SalesforceLead ToSalesforceLead(this RenaultLeadData renaultLead)
    {
        return new SalesforceLead
        {
            FirstName = renaultLead.Client.FirstName,
            LastName = renaultLead.Client.LastName,
            MobilePhone = renaultLead.Client.MobilePhone,
            Email = renaultLead.Client.Email,
            ModelOfInterest = renaultLead.Vehicle.ModelOfInterest,
            RenaultLeadReferenceId = renaultLead.LeadReferenceId,
            LeadSource = "Renault"
        };
    }

    public static RenaultFunnelRequest ToRenaultFunnelRequest(this SalesforceLead salesforceLead)
    {
        return new RenaultFunnelRequest
        {
            Status = salesforceLead.Status ?? RenaultConstants.Status.Prospection,
            StatusUpdatedAt = DateTime.UtcNow.ToString(UtcFormat)
        };
    }

    public static RenaultProspectionRequest ToRenaultProspectionRequest(this SalesforceLead salesforceLead, string attendantName, string salesName, bool contactSuccess)
    {
        return new RenaultProspectionRequest
        {
            RequestDateUpdate = DateTime.UtcNow.ToString(UtcFormat),
            ProspectionLeadCreationDate = salesforceLead.CreatedDate?.ToString(UtcFormat) ?? DateTime.UtcNow.ToString(UtcFormat),
            ProspectionAttendantName = attendantName,
            ProspectionSalesName = salesName,
            ProspectionContactSuccess = contactSuccess ? "S" : "N"
        };
    }

    // Phase 3: Exclusive Leads
    public static RenaultLeadData ToRenaultCreateRequest(this SalesforceLead salesforceLead)
    {
        // This is a bit different since the Renault API for Create might have a slightly different structure or fields
        // But following the provided mapping "De (Salesforce) Para (Renault)":
        return new RenaultLeadData
        {
            Client = new RenaultClient
            {
                FirstName = salesforceLead.FirstName ?? string.Empty,
                LastName = salesforceLead.LastName ?? string.Empty,
                MobilePhone = salesforceLead.MobilePhone ?? string.Empty,
                Email = salesforceLead.Email ?? string.Empty
            },
            SubmissionTimestamp = salesforceLead.CreatedDate?.ToString(UtcFormat) ?? DateTime.UtcNow.ToString(UtcFormat)
            // Note: Other fields like leadProvider, typeOfInterest would be added if the RenaultLeadData is used for Create
        };
    }
}
