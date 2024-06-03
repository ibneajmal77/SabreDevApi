using FlightSearchApp.Domain.Models;
using FluentValidation;

namespace FlightSearchApp.Domain.Validators
{
    public class FlightSearchRequestValidator : AbstractValidator<FlightSearchRequest>
    {
        public FlightSearchRequestValidator()
        {
            RuleFor(x => x.Origin).NotEmpty().WithMessage("Origin is required.");
            RuleFor(x => x.Destination).NotEmpty().WithMessage("Destination is required.");
            RuleFor(x => x.DepartureDate).NotEmpty().WithMessage("Departure Date is required.");
        }
    }
}
