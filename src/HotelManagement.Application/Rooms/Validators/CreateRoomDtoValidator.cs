using FluentValidation;
using HotelManagement.Application.Rooms.Dtos;

namespace HotelManagement.Application.Rooms.Validators;

public sealed class CreateRoomDtoValidator : AbstractValidator<CreateRoomDto>
{
    public CreateRoomDtoValidator()
    {
        RuleFor(dto => dto.Number).NotEmpty().MaximumLength(20);
        RuleFor(dto => dto.Capacity).InclusiveBetween(1, 20);
        RuleFor(dto => dto.Type).IsInEnum();
        RuleFor(dto => dto.Status).IsInEnum();
    }
}
