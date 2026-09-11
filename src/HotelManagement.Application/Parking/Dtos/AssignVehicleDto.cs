using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Parking.Dtos;

public sealed class AssignVehicleDto
{
    [Required]
    [MaxLength(20)]
    public string RegistrationNumber { get; init; } = string.Empty;
}
