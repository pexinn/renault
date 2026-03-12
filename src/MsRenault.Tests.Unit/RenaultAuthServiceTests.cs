using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using MsRenault.Dominio.DTOs.Renault;
using MsRenault.Infra.Dados.Services;
using FluentAssertions;
using Xunit;

namespace MsRenault.Tests.Unit;

public class RenaultAuthServiceTests
{
    private readonly Mock<HttpMessageHandler> _msgHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<RenaultAuthService>> _loggerMock;

    public RenaultAuthServiceTests()
    {
        _msgHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_msgHandlerMock.Object);
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<RenaultAuthService>>();

        _configMock.Setup(x => x["Renault:AccessKey"]).Returns("key");
        _configMock.Setup(x => x["Renault:Password"]).Returns("pass");
        _configMock.Setup(x => x["Renault:BaseUrl"]).Returns("https://api.test.com");
    }

    [Fact]
    public async Task GetAccessTokenAsync_ShouldReturnToken_AndCacheIt()
    {
        // Arrange
        var authResponse = new RenaultAuthResponse
        {
            AccessToken = "TOKEN_123",
            SecretKey = "SECRET_123",
            ExpiresIn = 7776000 // 90 days
        };

        _msgHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(authResponse)
            });

        var service = new RenaultAuthService(_httpClient, _configMock.Object, _loggerMock.Object);

        // Act
        var token1 = await service.GetAccessTokenAsync();
        var token2 = await service.GetAccessTokenAsync();

        // Assert
        token1.Should().Be("TOKEN_123");
        token2.Should().Be("TOKEN_123");
        
        // Verify only 1 HTTP call was made (caching)
        _msgHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
