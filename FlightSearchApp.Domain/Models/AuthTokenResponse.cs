namespace FlightSearchApp.Domain.Models
{
    public class AuthTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }
}
