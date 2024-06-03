using FlightSearchApp.Domain.Entities;

namespace FlightSearchApp.Domain.Interfaces
{
    public interface IFlightService
    {
        Task<IEnumerable<FlightSearchResponse>> SearchFlightsAsync(string origin, string destination, DateTime departureDate, DateTime? returnDate = null, bool isRoundTrip = false, bool isMultiCity = false, List<(string origin, string destination, DateTime departureDate)>? multiCitySegments = null);
    }
}
