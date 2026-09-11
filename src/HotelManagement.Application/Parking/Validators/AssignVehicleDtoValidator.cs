using FluentValidation;
using HotelManagement.Application.Parking.Dtos;
using HotelManagement.Application.Reservations;

namespace HotelManagement.Application.Parking.Validators;

public sealed class AssignVehicleDtoValidator : AbstractValidator<AssignVehicleDto>
{
    public AssignVehicleDtoValidator()
    {
        RuleFor(dto => dto.RegistrationNumber)
            .NotEmpty()
            .MaximumLength(20)
            .Must(LicensePlateFormatter.IsValid)
            .WithMessage("Registration number must contain 2 to 15 letters or digits. Spaces and hyphens are allowed.");
    }
}
