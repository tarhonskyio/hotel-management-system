using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Rooms.Dtos;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Rooms;

public sealed class RoomService : IRoomService
{
    private readonly IHotelRepository _repository;

    public RoomService(IHotelRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RoomResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rooms = await _repository.GetRoomsAsync(cancellationToken);
        return rooms.Select(ToResponse).ToList();
    }

    public async Task<RoomResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var room = await GetRoomOrThrowAsync(id, cancellationToken);
        return ToResponse(room);
    }

    public async Task<RoomResponseDto> CreateAsync(CreateRoomDto dto, CancellationToken cancellationToken)
    {
        var number = NormalizeRoomNumber(dto.Number);
        var existingRoom = await _repository.GetRoomByNumberAsync(number, cancellationToken);
        if (existingRoom is not null)
        {
            throw new ConflictException($"Room '{number}' already exists.");
        }

        var room = new Room
        {
            Number = number,
            Type = dto.Type,
            Capacity = dto.Capacity,
            Status = dto.Status
        };

        await _repository.AddRoomAsync(room, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToResponse(room);
    }

    public async Task<RoomResponseDto> UpdateAsync(Guid id, UpdateRoomDto dto, CancellationToken cancellationToken)
    {
        var room = await GetRoomOrThrowAsync(id, cancellationToken);

        if (dto.Number is not null)
        {
            var number = NormalizeRoomNumber(dto.Number);
            var existingRoom = await _repository.GetRoomByNumberAsync(number, cancellationToken);
            if (existingRoom is not null && existingRoom.Id != id)
            {
                throw new ConflictException($"Room '{number}' already exists.");
            }

            room.Number = number;
        }

        room.Type = dto.Type ?? room.Type;
        room.Capacity = dto.Capacity ?? room.Capacity;
        room.Status = dto.Status ?? room.Status;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToResponse(room);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var room = await GetRoomOrThrowAsync(id, cancellationToken);
        _repository.RemoveRoom(room);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Room> GetRoomOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var room = await _repository.GetRoomByIdAsync(id, cancellationToken);
        return room ?? throw new NotFoundException($"Room '{id}' was not found.");
    }

    private static RoomResponseDto ToResponse(Room room)
    {
        return new RoomResponseDto
        {
            Id = room.Id,
            Number = room.Number,
            Type = room.Type,
            Capacity = room.Capacity,
            Status = room.Status
        };
    }

    private static string NormalizeRoomNumber(string number)
    {
        return number.Trim().ToUpperInvariant();
    }
}
