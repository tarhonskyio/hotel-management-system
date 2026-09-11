using FluentValidation;
using HotelManagement.Application.Reservations.Dtos;

namespace HotelManagement.Application.Reservations.Validators;

public sealed class CreateReservationDtoValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationDtoValidator()
    {
        RuleFor(dto => dto.GuestFirstName).NotEmpty().MaximumLength(80);
        RuleFor(dto => dto.GuestLastName).NotEmpty().MaximumLength(80);
        RuleFor(dto => dto.Email).NotEmpty().EmailAddress().MaximumLength(160);
        RuleFor(dto => dto.RoomId).NotEmpty();
        RuleFor(dto => dto.Status).IsInEnum();
        RuleFor(dto => dto.CheckOut)
            .GreaterThan(dto => dto.CheckIn)
            .WithMessage("Check-out date must be after check-in date.");
    }
}
