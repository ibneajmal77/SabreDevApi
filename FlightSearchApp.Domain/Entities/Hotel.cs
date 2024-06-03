namespace FlightSearchApp.Domain.Entities
{
    public class Hotel
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public decimal? PricePerNight { get; set; }
        public int? Rating { get; set; }
    }
}
