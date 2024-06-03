using FlightSearchApp.Domain.Entities;

namespace FlightSearchApp.Domain.Interfaces
{
    public interface ISabreApiClient
    {
        Task<IEnumerable<FlightSearchResponse>> SearchFlightsAsync(string origin, string destination, DateTime departureDate);
        Task<IEnumerable<Hotel>> SearchHotelsAsync(string location, DateTime checkInDate, DateTime checkOutDate);
    }
}
