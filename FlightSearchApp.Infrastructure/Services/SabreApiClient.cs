using FlightSearchApp.Domain.Entities;
using FlightSearchApp.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlightSearchApp.Infrastructure.Services
{
    public class SabreApiClient : ISabreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _authTokenUrl;
        private readonly string _flightSearchUrl;
        private readonly string _hotelSearchUrl;
        private readonly ILogger<SabreApiClient> _logger;
        private string? _authToken;
        private DateTime _tokenExpiryTime;

        public SabreApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<SabreApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _clientId = configuration["SabreApi:ClientId"] ?? throw new ArgumentNullException(nameof(configuration), "Client ID not found in configuration");
            _clientSecret = configuration["SabreApi:ClientSecret"] ?? throw new ArgumentNullException(nameof(configuration), "Client Secret not found in configuration");
            _authTokenUrl = configuration["SabreApi:AuthTokenUrl"] ?? throw new ArgumentNullException(nameof(configuration), "Auth Token URL not found in configuration");
            _flightSearchUrl = configuration["SabreApi:FlightSearchUrl"] ?? throw new ArgumentNullException(nameof(configuration), "Flight Search URL not found in configuration");
            _hotelSearchUrl = configuration["SabreApi:HotelSearchUrl"] ?? throw new ArgumentNullException(nameof(configuration), "Hotel Search URL not found in configuration");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var requestBody = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials")
                    };

                    var requestContent = new FormUrlEncodedContent(requestBody);

                    var response = await httpClient.PostAsync(_authTokenUrl, requestContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to obtain access token. Status Code: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                        response.EnsureSuccessStatusCode();
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);

                    _tokenExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                    return tokenResponse.AccessToken;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while obtaining the access token.");
                throw;
            }
        }

        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }

        private async Task EnsureValidTokenAsync()
        {
            if (string.IsNullOrEmpty(_authToken) || DateTime.UtcNow >= _tokenExpiryTime)
            {
                _authToken = await GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            }
        }

        public async Task<IEnumerable<FlightSearchResponse>> SearchFlightsAsync(string origin, string destination, DateTime departureDate)
        {
            await EnsureValidTokenAsync();

            var requestUri = $"{_flightSearchUrl}?origin={origin}&destination={destination}&departuredate={departureDate:yyyy-MM-dd}";
            return await GetAsync<IEnumerable<FlightSearchResponse>>(requestUri);
        }

        public async Task<IEnumerable<Hotel>> SearchHotelsAsync(string location, DateTime checkInDate, DateTime checkOutDate)
        {
            await EnsureValidTokenAsync();

            var requestUri = $"{_hotelSearchUrl}?location={location}&checkInDate={checkInDate:yyyy-MM-dd}&checkOutDate={checkOutDate:yyyy-MM-dd}";
            return await GetAsync<IEnumerable<Hotel>>(requestUri);
        }

        private async Task<T> GetAsync<T>(string requestUri)
        {
            try
            {
                var response = await _httpClient.GetAsync(requestUri);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Token expired, requesting a new token...");
                    await EnsureValidTokenAsync();
                    response = await _httpClient.GetAsync(requestUri);
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("Received empty response for request: {RequestUri}", requestUri);
                    return default!;
                }

                var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogWarning("Deserialization returned null for request: {RequestUri}", requestUri);
                    return default!;
                }

                return result;
            }
            catch (HttpRequestException httpRequestException)
            {
                _logger.LogError(httpRequestException, "An error occurred when calling the API: {Message}", httpRequestException.Message);
                throw new ApplicationException($"An error occurred when calling the API: {httpRequestException.Message}", httpRequestException);
            }
            catch (JsonException jsonException)
            {
                _logger.LogError(jsonException, "An error occurred when deserializing the response: {Message}", jsonException.Message);
                throw new ApplicationException($"An error occurred when deserializing the response: {jsonException.Message}", jsonException);
            }
        }
    }
}
