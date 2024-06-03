using FlightSearchApp.Domain.Entities;
using FlightSearchApp.Domain.Interfaces;

namespace FlightSearchApp.Infrastructure.Services
{
    public class FlightService : IFlightService
    {
        private readonly ISabreApiClient _sabreApiClient;

        public FlightService(ISabreApiClient sabreApiClient)
        {
            _sabreApiClient = sabreApiClient;
        }

        public async Task<IEnumerable<FlightSearchResponse>> SearchFlightsAsync(string origin, string destination, DateTime departureDate, DateTime? returnDate = null, bool isRoundTrip = false, bool isMultiCity = false, List<(string origin, string destination, DateTime departureDate)>? multiCitySegments = null)
        {
            if (isMultiCity && multiCitySegments != null)
            {
                // Handle multi-city flight search
                var flights = new List<FlightSearchResponse>();
                foreach (var segment in multiCitySegments)
                {
                    var segmentFlights = await _sabreApiClient.SearchFlightsAsync(segment.origin, segment.destination, segment.departureDate);
                    flights.AddRange(segmentFlights);
                }
                return flights;
            }
            else if (isRoundTrip && returnDate.HasValue)
            {
                // Handle round-trip flight search
                var outboundFlights = await _sabreApiClient.SearchFlightsAsync(origin, destination, departureDate);
                var returnFlights = await _sabreApiClient.SearchFlightsAsync(destination, origin, returnDate.Value);
                return outboundFlights; // Or handle combining the two sets of flights
            }
            else
            {
                // Handle one-way flight search
                return await _sabreApiClient.SearchFlightsAsync(origin, destination, departureDate);
            }
        }
    }
}
