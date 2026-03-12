using FluentAssertions;
using MsRenault.Dominio.Constants;
using MsRenault.Dominio.DTOs.Renault;
using MsRenault.Dominio.DTOs.Salesforce;
using MsRenault.Dominio.Mappers;
using Xunit;

namespace MsRenault.Tests.Unit;

public class LeadMapperTests
{
    [Fact]
    public void ToSalesforceLead_ShouldMapCorrectly()
    {
        // Arrange
        var renaultLead = new RenaultLeadData
        {
            LeadReferenceId = "REF123",
            Client = new RenaultClient
            {
                FirstName = "Joao",
                LastName = "Silva",
                Email = "joao@example.com",
                MobilePhone = "123456789"
            },
            Vehicle = new RenaultVehicle
            {
                ModelOfInterest = "Renault Kwid"
            }
        };

        // Act
        var result = renaultLead.ToSalesforceLead();

        // Assert
        result.FirstName.Should().Be("Joao");
        result.LastName.Should().Be("Silva");
        result.Email.Should().Be("joao@example.com");
        result.MobilePhone.Should().Be("123456789");
        result.ModelOfInterest.Should().Be("Renault Kwid");
        result.RenaultLeadReferenceId.Should().Be("REF123");
        result.LeadSource.Should().Be("Renault");
    }

    [Fact]
    public void ToRenaultFunnelRequest_ShouldMapCorrectlyWithUtcDate()
    {
        // Arrange
        var salesforceLead = new SalesforceLead
        {
            Status = RenaultConstants.Status.Negotiation
        };

        // Act
        var result = salesforceLead.ToRenaultFunnelRequest();

        // Assert
        result.Status.Should().Be(RenaultConstants.Status.Negotiation);
        result.StatusUpdatedAt.Should().EndWith("Z");
        DateTime.TryParse(result.StatusUpdatedAt, out _).Should().BeTrue();
    }

    [Fact]
    public void ToRenaultProspectionRequest_ShouldMapCorrectly()
    {
        // Arrange
        var salesforceLead = new SalesforceLead
        {
            CreatedDate = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = salesforceLead.ToRenaultProspectionRequest("AttendantX", "SalesY", true);

        // Assert
        result.ProspectionAttendantName.Should().Be("AttendantX");
        result.ProspectionSalesName.Should().Be("SalesY");
        result.ProspectionContactSuccess.Should().Be("S");
        result.ProspectionLeadCreationDate.Should().Be("2025-01-01T10:00:00Z");
    }
}
