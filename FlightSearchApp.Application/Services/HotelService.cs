using FlightSearchApp.Domain.Entities;
using FlightSearchApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlightSearchApp.Application.Services
{
    public class HotelService : IHotelService
    {
        private readonly ISabreApiClient _sabreApiClient;
        private readonly ILogger<HotelService> _logger;

        public HotelService(ISabreApiClient sabreApiClient, ILogger<HotelService> logger)
        {
            _sabreApiClient = sabreApiClient;
            _logger = logger;
        }

        public async Task<IEnumerable<Hotel>> SearchHotelsAsync(string location, DateTime checkInDate, DateTime checkOutDate)
        {
            try
            {
                return await _sabreApiClient.SearchHotelsAsync(location, checkInDate, checkOutDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for hotels in {Location} from {CheckInDate} to {CheckOutDate}", location, checkInDate, checkOutDate);
                throw;
            }
        }
    }
}
