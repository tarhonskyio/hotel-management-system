using FluentValidation;
using FluentValidation.Results;
using HotelManagement.Application.Reservations;
using HotelManagement.Application.Reservations.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Api.Controllers;

/// <summary>
/// Manages hotel reservations and linked vehicle parking access.
/// </summary>
[Authorize]
[ApiController]
[Route("api/reservations")]
[Produces("application/json")]
public sealed class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly IValidator<CreateReservationDto> _createReservationValidator;
    private readonly IValidator<UpdateReservationDto> _updateReservationValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReservationsController"/> class.
    /// </summary>
    public ReservationsController(
        IReservationService reservationService,
        IValidator<CreateReservationDto> createReservationValidator,
        IValidator<UpdateReservationDto> updateReservationValidator)
    {
        _reservationService = reservationService;
        _createReservationValidator = createReservationValidator;
        _updateReservationValidator = updateReservationValidator;
    }

    /// <summary>
    /// Creates a new hotel reservation.
    /// </summary>
    /// <param name="request">Reservation creation payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The created reservation.</returns>
    /// <response code="201">The reservation was created.</response>
    /// <response code="400">The request payload or business data is invalid.</response>
    /// <response code="401">The request is missing a valid JWT bearer token.</response>
    /// <response code="409">The room is already reserved for the requested dates.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponseDto>> CreateAsync(
        [FromBody] CreateReservationDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createReservationValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ToValidationProblem(validationResult);
        }

        var reservation = await _reservationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = reservation.Id }, reservation);
    }

    /// <summary>
    /// Gets all non-cancelled reservations.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>All non-cancelled reservations that have not checked out yet.</returns>
    /// <response code="200">Returns active reservations.</response>
    /// <response code="401">The request is missing a valid JWT bearer token.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ReservationResponseDto>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var reservations = await _reservationService.GetActiveAsync(cancellationToken);
        return Ok(reservations);
    }

    /// <summary>
    /// Gets a reservation by its identifier.
    /// </summary>
    /// <param name="id">Reservation identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The matching reservation.</returns>
    /// <response code="200">Returns the requested reservation.</response>
    /// <response code="401">The request is missing a valid JWT bearer token.</response>
    /// <response code="404">The reservation does not exist.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var reservation = await _reservationService.GetByIdAsync(id, cancellationToken);
        return Ok(reservation);
    }

    /// <summary>
    /// Updates reservation details.
    /// </summary>
    /// <param name="id">Reservation identifier.</param>
    /// <param name="request">Reservation update payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The updated reservation.</returns>
    /// <response code="200">The reservation was updated.</response>
    /// <response code="400">The request payload or business data is invalid.</response>
    /// <response code="401">The request is missing a valid JWT bearer token.</response>
    /// <response code="404">The reservation does not exist.</response>
    /// <response code="409">The room is already reserved for the requested dates.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponseDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateReservationDto request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateReservationValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ToValidationProblem(validationResult);
        }

        var reservation = await _reservationService.UpdateAsync(id, request, cancellationToken);
        return Ok(reservation);
    }

    /// <summary>
    /// Soft-cancels a reservation.
    /// </summary>
    /// <param name="id">Reservation identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>No content when the cancellation succeeds.</returns>
    /// <response code="204">The reservation was cancelled.</response>
    /// <response code="401">The request is missing a valid JWT bearer token.</response>
    /// <response code="404">The reservation does not exist.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        await _reservationService.CancelAsync(id, cancellationToken);
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
