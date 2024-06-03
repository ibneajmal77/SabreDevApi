using FlightSearchApp.Domain.Entities;

namespace FlightSearchApp.Domain.Interfaces
{
    public interface IHotelService
    {
        Task<IEnumerable<Hotel>> SearchHotelsAsync(string location, DateTime checkInDate, DateTime checkOutDate);
    }
}
