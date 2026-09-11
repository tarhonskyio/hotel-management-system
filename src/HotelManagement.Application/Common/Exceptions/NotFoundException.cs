namespace HotelManagement.Application.Common.Exceptions;

/// <summary>
/// Represents a missing resource.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException(string message)
        : base(message)
    {
    }
}
