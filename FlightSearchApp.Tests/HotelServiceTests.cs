using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightSearchApp.Domain.Entities;
using FlightSearchApp.Domain.Interfaces;
using FlightSearchApp.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlightSearchApp.Tests
{
    public class HotelServiceTests
    {
        private readonly Mock<ISabreApiClient> _sabreApiClientMock;
        private readonly Mock<ILogger<HotelService>> _loggerMock;
        private readonly HotelService _hotelService;

        public HotelServiceTests()
        {
            _sabreApiClientMock = new Mock<ISabreApiClient>();
            _loggerMock = new Mock<ILogger<HotelService>>();
            _hotelService = new HotelService(_sabreApiClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SearchHotelsAsync_ShouldReturnHotels()
        {
            // Arrange
            var location = "New York";
            var checkInDate = new DateTime(2024, 7, 1);
            var checkOutDate = new DateTime(2024, 7, 10);
            var hotels = new List<Hotel>
            {
                new Hotel { Name = "Hotel A", Location = location }
            };

            _sabreApiClientMock
                .Setup(client => client.SearchHotelsAsync(location, checkInDate, checkOutDate))
                .ReturnsAsync(hotels);

            // Act
            var result = await _hotelService.SearchHotelsAsync(location, checkInDate, checkOutDate);

            // Assert
            Assert.Equal(hotels, result);
        }
    }
}
