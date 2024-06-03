FlightSearchApp
FlightSearchApp is a .NET-based application designed to search for flights and hotels using the Sabre API. The application consists of multiple projects including a console application for user interaction, an infrastructure project for API communication, domain models for handling flight and hotel data, and unit tests for ensuring code quality.

Table of Contents
Prerequisites
Setup
Configuration
Usage
Testing
Logging
Project Structure
Contributing
License
Prerequisites
.NET 6 SDK or later
Visual Studio 2022 or later / Visual Studio Code
Sabre API credentials
Setup
Clone the repository:

sh
Copy code
git clone https://github.com/ibneajmal77/SabreDevApi.git
cd FlightSearchApp
Restore NuGet packages:

sh
Copy code
dotnet restore
Build the solution:

sh
Copy code
dotnet build
Configuration
Create an appsettings.json file in the FlightSearchApp.ConsoleApp project directory with the following content:
json
Copy code
{
  "SabreApi": {
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret",
    "AuthTokenUrl": "https://api-crt.cert.havail.sabre.com/v2/auth/token",
    "FlightSearchUrl": "https://api-crt.cert.havail.sabre.com/v1/shop/flights",
    "HotelSearchUrl": "https://api-crt.cert.havail.sabre.com/v1/shop/hotels"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
Replace your_client_id and your_client_secret with your actual Sabre API credentials.
Usage
Run the console application:

sh
Copy code
cd FlightSearchApp.ConsoleApp
dotnet run
Follow the on-screen prompts to search for flights or hotels.

For flight search, you can choose between one-way, round-trip, or multi-city searches.
For hotel search, provide the location, check-in date, and check-out date.
Testing
Navigate to the FlightSearchApp.Tests project directory.
Run the tests:
sh
Copy code
dotnet test
Logging
The application uses Serilog for logging. Logs are written to the console and to a file located at FlightSearchApp.ConsoleApp/logs/log.txt.

Project Structure
plaintext
Copy code
FlightSearchApp/
│
├── FlightSearchApp.Application/
│   └── Services/
│       ├── FlightService.cs
│       └── HotelService.cs
│
├── FlightSearchApp.ConsoleApp/
│   ├── appsettings.json
│   └── Program.cs
│
├── FlightSearchApp.Domain/
│   ├── Entities/
│   │   ├── Flight.cs
│   │   └── Hotel.cs
│   ├── Interfaces/
│   │   ├── IFlightService.cs
│   │   ├── IHotelService.cs
│   │   └── ISabreApiClient.cs
│   ├── Models/
│   │   ├── AuthTokenResponse.cs
│   │   ├── FlightSearchRequest.cs
│   │   ├── FlightSearchResponse.cs
│   │   └── HotelSearchRequest.cs
│   └── Validators/
│       ├── FlightSearchRequestValidator.cs
│       └── HotelSearchRequestValidator.cs
│
├── FlightSearchApp.Infrastructure/
│   └── Services/
│       └── SabreApiClient.cs
│
└── FlightSearchApp.Tests/
    ├── FlightServiceTests.cs
    ├── HotelServiceTests.cs
    └── SabreApiClientTests.cs
Contributing
Contributions are welcome! Please fork the repository and create a pull request with your changes.