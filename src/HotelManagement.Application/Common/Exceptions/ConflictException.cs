namespace HotelManagement.Application.Common.Exceptions;

/// <summary>
/// Represents a business conflict that prevents the requested operation.
/// </summary>
public sealed class ConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    public ConflictException(string message)
        : base(message)
    {
    }
}
