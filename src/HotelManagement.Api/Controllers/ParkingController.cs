using FluentValidation;
using FluentValidation.Results;
using HotelManagement.Application.Parking;
using HotelManagement.Application.Parking.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Api.Controllers;

/// <summary>
/// Manages vehicle parking access for reservations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/parking")]
[Produces("application/json")]
public sealed class ParkingController : ControllerBase
{
    private readonly IParkingService _parkingService;
    private readonly IValidator<AssignVehicleDto> _assignVehicleValidator;

    public ParkingController(IParkingService parkingService, IValidator<AssignVehicleDto> assignVehicleValidator)
    {
        _parkingService = parkingService;
        _assignVehicleValidator = assignVehicleValidator;
    }

    /// <summary>
    /// Assigns or updates a vehicle registration number for a reservation.
    /// </summary>
    [HttpPost("reservations/{reservationId:guid}/vehicles")]
    [ProducesResponseType(typeof(ParkingAccessResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParkingAccessResponseDto>> AssignVehicleAsync(
        Guid reservationId,
        [FromBody] AssignVehicleDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _assignVehicleValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ToValidationProblem(validationResult);
        }

        return Ok(await _parkingService.AssignVehicleAsync(reservationId, request, cancellationToken));
    }

    /// <summary>
    /// Gets parking access records for a reservation.
    /// </summary>
    [HttpGet("reservations/{reservationId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ParkingAccessResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ParkingAccessResponseDto>>> GetReservationParkingAccessAsync(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        return Ok(await _parkingService.GetReservationParkingAccessAsync(reservationId, cancellationToken));
    }

    /// <summary>
    /// Verifies whether a registration number currently has valid parking access.
    /// </summary>
    [HttpGet("access/{registrationNumber}")]
    [ProducesResponseType(typeof(ParkingVerificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParkingVerificationResponseDto>> VerifyAccessAsync(
        string registrationNumber,
        CancellationToken cancellationToken)
    {
        return Ok(await _parkingService.VerifyAccessAsync(registrationNumber, cancellationToken));
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
