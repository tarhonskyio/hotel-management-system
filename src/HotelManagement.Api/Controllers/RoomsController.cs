using FluentValidation;
using FluentValidation.Results;
using HotelManagement.Application.Rooms;
using HotelManagement.Application.Rooms.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Api.Controllers;

/// <summary>
/// Manages hotel rooms.
/// </summary>
[Authorize]
[ApiController]
[Route("api/rooms")]
[Produces("application/json")]
public sealed class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IValidator<CreateRoomDto> _createValidator;
    private readonly IValidator<UpdateRoomDto> _updateValidator;

    public RoomsController(
        IRoomService roomService,
        IValidator<CreateRoomDto> createValidator,
        IValidator<UpdateRoomDto> updateValidator)
    {
        _roomService = roomService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Gets all rooms.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoomResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoomResponseDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Ok(await _roomService.GetAllAsync(cancellationToken));
    }

    /// <summary>
    /// Gets one room by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoomResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _roomService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Creates a new room.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoomResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomResponseDto>> CreateAsync(
        [FromBody] CreateRoomDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ToValidationProblem(validationResult);
        }

        var room = await _roomService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = room.Id }, room);
    }

    /// <summary>
    /// Updates a room.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoomResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomResponseDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateRoomDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ToValidationProblem(validationResult);
        }

        return Ok(await _roomService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Deletes a room.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _roomService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private ActionResult ToValidationProblem(ValidationResult validationResult)
    {
        foreach (var error in validationResult.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return ValidationProblem(ModelState);
    }
}
