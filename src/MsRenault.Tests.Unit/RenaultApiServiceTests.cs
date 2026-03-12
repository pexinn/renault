using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using MsRenault.Dominio.DTOs.Renault;
using MsRenault.Dominio.Interfaces;
using MsRenault.Infra.Dados.Services;
using FluentAssertions;
using Xunit;

namespace MsRenault.Tests.Unit;

public class RenaultApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _msgHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IRenaultAuthService> _authMock;
    private readonly Mock<IConfiguration> _configMock;

    public RenaultApiServiceTests()
    {
        _msgHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_msgHandlerMock.Object);
        _authMock = new Mock<IRenaultAuthService>();
        _configMock = new Mock<IConfiguration>();

        _authMock.Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("valid_token");
        _authMock.Setup(x => x.GetSecretKeyAsync(It.IsAny<CancellationToken>())).ReturnsAsync("valid_secret");
        _configMock.Setup(x => x["Renault:BaseUrl"]).Returns("https://api.test.com");
    }

    [Fact]
    public async Task ConsumeLeadsAsync_ShouldSendCorrectRequest()
    {
        // Arrange
        var apiResponse = new RenaultConsumeResponse
        {
            Data = new List<RenaultLeadData> { new() { LeadReferenceId = "L1" } },
            Total = 1
        };

        _msgHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/v1/leads/consume") &&
                    req.Headers.Authorization!.Parameter == "valid_token" &&
                    req.Headers.Contains("SecretKey")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(apiResponse)
            });

        var service = new RenaultApiService(_httpClient, _authMock.Object, _configMock.Object);

        // Act
        var result = await service.ConsumeLeadsAsync("BIR01", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Assert
        result.Total.Should().Be(1);
        result.Data.Should().HaveCount(1);
    }
}
