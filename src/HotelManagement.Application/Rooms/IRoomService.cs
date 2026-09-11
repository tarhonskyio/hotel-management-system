using HotelManagement.Application.Rooms.Dtos;

namespace HotelManagement.Application.Rooms;

public interface IRoomService
{
    Task<IReadOnlyList<RoomResponseDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<RoomResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<RoomResponseDto> CreateAsync(CreateRoomDto dto, CancellationToken cancellationToken);

    Task<RoomResponseDto> UpdateAsync(Guid id, UpdateRoomDto dto, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
