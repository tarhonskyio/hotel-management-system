using FluentValidation;
using HotelManagement.Application.Rooms.Dtos;

namespace HotelManagement.Application.Rooms.Validators;

public sealed class UpdateRoomDtoValidator : AbstractValidator<UpdateRoomDto>
{
    public UpdateRoomDtoValidator()
    {
        RuleFor(dto => dto.Number).NotEmpty().MaximumLength(20).When(dto => dto.Number is not null);
        RuleFor(dto => dto.Capacity).InclusiveBetween(1, 20).When(dto => dto.Capacity.HasValue);
        RuleFor(dto => dto.Type).IsInEnum().When(dto => dto.Type.HasValue);
        RuleFor(dto => dto.Status).IsInEnum().When(dto => dto.Status.HasValue);
    }
}
