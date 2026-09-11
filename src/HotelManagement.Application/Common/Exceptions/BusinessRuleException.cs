namespace HotelManagement.Application.Common.Exceptions;

/// <summary>
/// Represents a request that is syntactically valid but violates domain business rules.
/// </summary>
public sealed class BusinessRuleException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class.
    /// </summary>
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}
