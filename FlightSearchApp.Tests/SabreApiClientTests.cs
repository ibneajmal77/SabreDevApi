using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FlightSearchApp.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace FlightSearchApp.Tests
{
    public class SabreApiClientTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<SabreApiClient>> _loggerMock;
        private readonly SabreApiClient _sabreApiClient;

        public SabreApiClientTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://api-crt.cert.havail.sabre.com/")
            };
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<SabreApiClient>>();

            _configurationMock.SetupGet(x => x["SabreApi:ClientId"]).Returns("clientId");
            _configurationMock.SetupGet(x => x["SabreApi:ClientSecret"]).Returns("clientSecret");
            _configurationMock.SetupGet(x => x["SabreApi:AuthTokenUrl"]).Returns("https://api-crt.cert.havail.sabre.com/v2/auth/token");
            _configurationMock.SetupGet(x => x["SabreApi:FlightSearchUrl"]).Returns("https://api-crt.cert.havail.sabre.com/v2/shop/flights");
            _configurationMock.SetupGet(x => x["SabreApi:HotelSearchUrl"]).Returns("https://api-crt.cert.havail.sabre.com/v2/shop/hotels");

            _sabreApiClient = new SabreApiClient(_httpClient, _configurationMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ReturnsAccessToken()
        {
            // Arrange
            var expectedToken = "expectedAccessToken";
            var tokenResponse = new SabreApiClient.TokenResponse
            {
                AccessToken = expectedToken,
                ExpiresIn = 3600
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponse)),
                })
                .Verifiable();

            // Act
            var actualToken = await _sabreApiClient.GetAccessTokenAsync();

            // Assert
            Assert.Equal(expectedToken, actualToken);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("https://api-crt.cert.havail.sabre.com/v2/auth/token") &&
                    req.Headers.Authorization.Parameter == Convert.ToBase64String(Encoding.UTF8.GetBytes("clientId:clientSecret"))
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task EnsureValidTokenAsync_RefreshesTokenIfExpired()
        {
            // Arrange
            var expectedToken = "newAccessToken";
            var tokenResponse = new SabreApiClient.TokenResponse
            {
                AccessToken = expectedToken,
                ExpiresIn = 3600
            };

            _handlerMock
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponse)),
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            await _sabreApiClient.SearchFlightsAsync("NYC", "LAX", DateTime.UtcNow);

            // Assert
            Assert.Equal(expectedToken, _httpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public async Task GetAsync_RetriesOnUnauthorized()
        {
            // Arrange
            var initialToken = "initialToken";
            var newToken = "newAccessToken";
            var tokenResponse = new SabreApiClient.TokenResponse
            {
                AccessToken = newToken,
                ExpiresIn = 3600
            };

            _handlerMock
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            // Act
            var result = await _sabreApiClient.SearchFlightsAsync("NYC", "LAX", DateTime.UtcNow);

            // Assert
            Assert.Empty(result);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(3),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
