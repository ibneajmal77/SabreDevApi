using FlightSearchApp.Application.Services;
using FlightSearchApp.Domain.Entities;
using FlightSearchApp.Domain.Interfaces;
using FlightSearchApp.Domain.Models;
using FlightSearchApp.Domain.Validators;
using FlightSearchApp.Infrastructure.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FlightSearchApp.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Configure HttpClient for ISabreApiClient
            services.AddHttpClient<ISabreApiClient, SabreApiClient>();

            // Add other services
            services.AddTransient<IFlightService, FlightService>();
            services.AddTransient<IHotelService, HotelService>();

            // Add logging
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));

            // Register validators manually
            services.AddTransient<IValidator<FlightSearchRequest>, FlightSearchRequestValidator>();
            services.AddTransient<IValidator<HotelSearchRequest>, HotelSearchRequestValidator>();

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var flightValidator = serviceProvider.GetRequiredService<IValidator<FlightSearchRequest>>();
            var hotelValidator = serviceProvider.GetRequiredService<IValidator<HotelSearchRequest>>();

            try
            {
                Console.WriteLine("Select flight search type (1: One-Way, 2: Round-Trip, 3: Multi-City):");
                var searchType = Console.ReadLine();

                if (searchType == "1")
                {
                    await SearchOneWayFlights(serviceProvider, flightValidator);
                }
                else if (searchType == "2")
                {
                    await SearchRoundTripFlights(serviceProvider, flightValidator);
                }
                else if (searchType == "3")
                {
                    await SearchMultiCityFlights(serviceProvider, flightValidator);
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }

                // Hotel search example
                var hotelRequest = new HotelSearchRequest();

                Console.WriteLine("Enter hotel location:");
                hotelRequest.Location = Console.ReadLine();

                Console.WriteLine("Enter check-in date (YYYY-MM-DD):");
                var checkInDateString = Console.ReadLine();
                if (DateTime.TryParse(checkInDateString, out DateTime checkInDate))
                {
                    hotelRequest.CheckInDate = checkInDate;
                }

                Console.WriteLine("Enter check-out date (YYYY-MM-DD):");
                var checkOutDateString = Console.ReadLine();
                if (DateTime.TryParse(checkOutDateString, out DateTime checkOutDate))
                {
                    hotelRequest.CheckOutDate = checkOutDate;
                }

                var hotelValidationResult = hotelValidator.Validate(hotelRequest);
                if (!hotelValidationResult.IsValid)
                {
                    foreach (var error in hotelValidationResult.Errors)
                    {
                        Console.WriteLine($"Validation error: {error.ErrorMessage}");
                    }
                    return;
                }

                if (!string.IsNullOrEmpty(hotelRequest.Location))
                {
                    var hotels = await serviceProvider.GetRequiredService<IHotelService>()
                    .SearchHotelsAsync(hotelRequest.Location, hotelRequest.CheckInDate, hotelRequest.CheckOutDate);

                    Console.WriteLine("Hotels:");
                    foreach (var hotel in hotels)
                    {
                        Console.WriteLine($"{hotel.Name} in {hotel.Location}, Price per night: {hotel.PricePerNight}, Rating: {hotel.Rating}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the application");
            }
        }

        private static async Task SearchOneWayFlights(IServiceProvider serviceProvider, IValidator<FlightSearchRequest> validator)
        {
            var flightRequest = new FlightSearchRequest();

            Console.WriteLine("Enter origin:");
            flightRequest.Origin = Console.ReadLine();

            Console.WriteLine("Enter destination:");
            flightRequest.Destination = Console.ReadLine();

            Console.WriteLine("Enter departure date (YYYY-MM-DD):");
            var departureDateString = Console.ReadLine();
            if (DateTime.TryParse(departureDateString, out DateTime departureDate))
            {
                flightRequest.DepartureDate = departureDate;
            }

            var flightValidationResult = validator.Validate(flightRequest);
            if (!flightValidationResult.IsValid)
            {
                foreach (var error in flightValidationResult.Errors)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                return;
            }

            if (!string.IsNullOrEmpty(flightRequest.Origin) && !string.IsNullOrEmpty(flightRequest.Destination))
            {
                var flights = await serviceProvider.GetRequiredService<IFlightService>()
                .SearchFlightsAsync(flightRequest.Origin, flightRequest.Destination, flightRequest.DepartureDate);

                DisplayAndSelectFlight(flights);
            }
        }

        private static async Task SearchRoundTripFlights(IServiceProvider serviceProvider, IValidator<FlightSearchRequest> validator)
        {
            var flightRequest = new FlightSearchRequest();

            Console.WriteLine("Enter origin:");
            flightRequest.Origin = Console.ReadLine();

            Console.WriteLine("Enter destination:");
            flightRequest.Destination = Console.ReadLine();

            Console.WriteLine("Enter departure date (YYYY-MM-DD):");
            var departureDateString = Console.ReadLine();
            if (DateTime.TryParse(departureDateString, out DateTime departureDate))
            {
                flightRequest.DepartureDate = departureDate;
            }

            Console.WriteLine("Enter return date (YYYY-MM-DD):");
            var returnDateString = Console.ReadLine();
            DateTime? returnDate = null;
            if (DateTime.TryParse(returnDateString, out DateTime rDate))
            {
                returnDate = rDate;
            }

            var flightValidationResult = validator.Validate(flightRequest);
            if (!flightValidationResult.IsValid)
            {
                foreach (var error in flightValidationResult.Errors)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                return;
            }

            if (!string.IsNullOrEmpty(flightRequest.Origin) && !string.IsNullOrEmpty(flightRequest.Destination))
            {
                var flights = await serviceProvider.GetRequiredService<IFlightService>()
                .SearchFlightsAsync(flightRequest.Origin, flightRequest.Destination, flightRequest.DepartureDate, returnDate, true);

                DisplayAndSelectFlight(flights);
            }
        }

        private static async Task SearchMultiCityFlights(IServiceProvider serviceProvider, IValidator<FlightSearchRequest> validator)
        {
            var multiCitySegments = new List<(string, string, DateTime)>();

            while (true)
            {
                Console.WriteLine("Enter origin (or 'done' to finish):");
                var origin = Console.ReadLine();
                if (origin?.ToLower() == "done")
                {
                    break;
                }

                Console.WriteLine("Enter destination:");
                var destination = Console.ReadLine();

                Console.WriteLine("Enter departure date (YYYY-MM-DD):");
                var departureDateString = Console.ReadLine();
                if (DateTime.TryParse(departureDateString, out DateTime departureDate))
                {
                    multiCitySegments.Add((origin, destination, departureDate));
                }
            }

            if (multiCitySegments.Count < 2)
            {
                Console.WriteLine("You must enter at least two segments for a multi-city search.");
                return;
            }

            var flights = await serviceProvider.GetRequiredService<IFlightService>()
                .SearchFlightsAsync(string.Empty, string.Empty, DateTime.MinValue, null, false, true, multiCitySegments);

            DisplayAndSelectFlight(flights);
        }

        private static void DisplayAndSelectFlight(IEnumerable<FlightSearchResponse> flights)
        {
            Console.WriteLine("Flights:");
            int index = 1;
            var flightList = new List<FlightSearchResponse>(flights);

            foreach (var flightResponse in flightList)
            {
                if (flightResponse.PricedItineraries == null)
                {
                    continue;
                }

                foreach (var itinerary in flightResponse.PricedItineraries)
                {
                    if (itinerary.AirItinerary?.OriginDestinationOptions?.OriginDestinationOption == null)
                    {
                        continue;
                    }

                    foreach (var originDestination in itinerary.AirItinerary.OriginDestinationOptions.OriginDestinationOption)
                    {
                        if (originDestination.FlightSegment == null)
                        {
                            continue;
                        }

                        foreach (var segment in originDestination.FlightSegment)
                        {
                            Console.WriteLine($"{index++}. Flight {segment.FlightNumber} from {segment.DepartureAirport?.LocationCode} to {segment.ArrivalAirport?.LocationCode} by {segment.MarketingAirline?.Code} on {segment.DepartureDateTime}");
                        }
                    }

                    var fare = itinerary.AirItineraryPricingInfo?.ItinTotalFare;
                    if (fare != null)
                    {
                        Console.WriteLine($"Price: {fare.CurrencyCode} {fare.Amount}");
                    }
                }
            }

            Console.WriteLine("Select a flight by number:");
            if (int.TryParse(Console.ReadLine(), out int selectedIndex) && selectedIndex > 0 && selectedIndex <= index - 1)
            {
                var selectedFlightResponse = flightList[(selectedIndex - 1) / flightList.Count];
                var selectedItinerary = selectedFlightResponse.PricedItineraries?[(selectedIndex - 1) % flightList.Count];

                if (selectedItinerary != null && selectedItinerary.AirItinerary?.OriginDestinationOptions?.OriginDestinationOption != null)
                {
                    var selectedSegment = selectedItinerary.AirItinerary.OriginDestinationOptions.OriginDestinationOption[0].FlightSegment?[(selectedIndex - 1) % selectedItinerary.AirItinerary.OriginDestinationOptions.OriginDestinationOption[0].FlightSegment.Count];

                    if (selectedSegment != null)
                    {
                        Console.WriteLine($"Selected Flight: {selectedSegment.FlightNumber} from {selectedSegment.DepartureAirport?.LocationCode} to {selectedSegment.ArrivalAirport?.LocationCode} by {selectedSegment.MarketingAirline?.Code} on {selectedSegment.DepartureDateTime}");
                        var fare = selectedItinerary.AirItineraryPricingInfo?.ItinTotalFare;
                        if (fare != null)
                        {
                            Console.WriteLine($"Price: {fare.CurrencyCode} {fare.Amount}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }
    }
}
