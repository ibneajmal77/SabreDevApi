using FlightSearchApp.Domain.Models;
using FluentValidation;

namespace FlightSearchApp.Domain.Validators
{
    public class HotelSearchRequestValidator : AbstractValidator<HotelSearchRequest>
    {
        public HotelSearchRequestValidator()
        {
            RuleFor(x => x.Location).NotEmpty().WithMessage("Location is required.");
            RuleFor(x => x.CheckInDate).NotEmpty().WithMessage("Check-In Date is required.");
            RuleFor(x => x.CheckOutDate).NotEmpty().WithMessage("Check-Out Date is required.")
                .GreaterThan(x => x.CheckInDate).WithMessage("Check-Out Date must be after Check-In Date.");
        }
    }
}
