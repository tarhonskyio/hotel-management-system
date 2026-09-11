using HotelManagement.Application.Common.Interfaces;

namespace HotelManagement.Infrastructure;

/// <summary>
/// System clock implementation backed by <see cref="DateTime.UtcNow"/>.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
