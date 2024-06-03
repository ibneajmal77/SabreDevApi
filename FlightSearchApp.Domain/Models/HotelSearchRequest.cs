namespace FlightSearchApp.Domain.Models
{
    public class HotelSearchRequest
    {
        public string? Location { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}
