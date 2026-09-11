namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Provides the current time to application services.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC timestamp.
    /// </summary>
    DateTime UtcNow { get; }
}
