using FluentValidation;
using HotelManagement.Application.Reservations.Dtos;

namespace HotelManagement.Application.Reservations.Validators;

public sealed class UpdateReservationDtoValidator : AbstractValidator<UpdateReservationDto>
{
    public UpdateReservationDtoValidator()
    {
        RuleFor(dto => dto.GuestFirstName).NotEmpty().MaximumLength(80).When(dto => dto.GuestFirstName is not null);
        RuleFor(dto => dto.GuestLastName).NotEmpty().MaximumLength(80).When(dto => dto.GuestLastName is not null);
        RuleFor(dto => dto.Email).EmailAddress().MaximumLength(160).When(dto => dto.Email is not null);
        RuleFor(dto => dto.RoomId).NotEmpty().When(dto => dto.RoomId.HasValue);
        RuleFor(dto => dto.Status).IsInEnum().When(dto => dto.Status.HasValue);
        RuleFor(dto => dto)
            .Must(dto => dto.CheckIn is null || dto.CheckOut is null || dto.CheckOut.Value.Date > dto.CheckIn.Value.Date)
            .WithMessage("Check-out date must be after check-in date when both dates are provided.");
    }
}
