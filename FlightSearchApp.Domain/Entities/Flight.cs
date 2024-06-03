using Newtonsoft.Json;

namespace FlightSearchApp.Domain.Entities
{
    public class Flight
    {
        public string? FlightNumber { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public decimal? Price { get; set; }
    }

    public class FlightSearchResponse
    {
        [JsonProperty("PricedItineraries")]
        public List<PricedItinerary>? PricedItineraries { get; set; }
        // Add other necessary properties here
    }

    public class PricedItinerary
    {
        [JsonProperty("AirItinerary")]
        public AirItinerary? AirItinerary { get; set; }

        [JsonProperty("SequenceNumber")]
        public int SequenceNumber { get; set; }

        [JsonProperty("AirItineraryPricingInfo")]
        public AirItineraryPricingInfo? AirItineraryPricingInfo { get; set; }
        // Add other necessary properties here
    }

    public class AirItinerary
    {
        [JsonProperty("OriginDestinationOptions")]
        public OriginDestinationOptions? OriginDestinationOptions { get; set; }
        // Add other necessary properties here
    }

    public class OriginDestinationOptions
    {
        [JsonProperty("OriginDestinationOption")]
        public List<OriginDestinationOption>? OriginDestinationOption { get; set; }
    }

    public class OriginDestinationOption
    {
        [JsonProperty("FlightSegment")]
        public List<FlightSegment>? FlightSegment { get; set; }
    }

    public class FlightSegment
    {
        [JsonProperty("DepartureAirport")]
        public Airport? DepartureAirport { get; set; }

        [JsonProperty("ArrivalAirport")]
        public Airport? ArrivalAirport { get; set; }

        [JsonProperty("MarketingAirline")]
        public Airline? MarketingAirline { get; set; }

        [JsonProperty("StopQuantity")]
        public int StopQuantity { get; set; }

        [JsonProperty("DepartureDateTime")]
        public string? DepartureDateTime { get; set; }

        [JsonProperty("ArrivalDateTime")]
        public string? ArrivalDateTime { get; set; }

        [JsonProperty("FlightNumber")]
        public int FlightNumber { get; set; }
    }

    public class Airport
    {
        [JsonProperty("LocationCode")]
        public string? LocationCode { get; set; }
    }

    public class Airline
    {
        [JsonProperty("Code")]
        public string? Code { get; set; }
    }

    public class AirItineraryPricingInfo
    {
        [JsonProperty("ItinTotalFare")]
        public Fare? ItinTotalFare { get; set; }
    }

    public class Fare
    {
        [JsonProperty("CurrencyCode")]
        public string? CurrencyCode { get; set; }

        [JsonProperty("DecimalPlaces")]
        public int DecimalPlaces { get; set; }

        [JsonProperty("Amount")]
        public decimal Amount { get; set; }
    }
}
