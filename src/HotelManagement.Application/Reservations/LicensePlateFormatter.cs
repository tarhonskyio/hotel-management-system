using System.Text;
using System.Text.RegularExpressions;

namespace HotelManagement.Application.Reservations;

/// <summary>
/// Normalizes and validates license plate values used by the parking module.
/// </summary>
public static partial class LicensePlateFormatter
{
    private const int MinimumLength = 2;
    private const int MaximumLength = 15;

    /// <summary>
    /// Normalizes a license plate by removing spaces and hyphens and converting letters to uppercase.
    /// </summary>
    public static string? Normalize(string? licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return null;
        }

        var builder = new StringBuilder(licensePlate.Length);
        foreach (var character in licensePlate.Trim())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToUpperInvariant(character));
                continue;
            }

            if (char.IsWhiteSpace(character) || character == '-')
            {
                continue;
            }

            return null;
        }

        return builder.Length == 0 ? null : builder.ToString();
    }

    /// <summary>
    /// Checks whether a normalized license plate follows the parking module format.
    /// </summary>
    public static bool IsNormalizedValid(string? normalizedLicensePlate)
    {
        return normalizedLicensePlate is not null
            && normalizedLicensePlate.Length >= MinimumLength
            && normalizedLicensePlate.Length <= MaximumLength
            && LicensePlateRegex().IsMatch(normalizedLicensePlate);
    }

    /// <summary>
    /// Checks whether a raw license plate can be normalized into a valid parking module format.
    /// </summary>
    public static bool IsValid(string? licensePlate)
    {
        var normalized = Normalize(licensePlate);
        return IsNormalizedValid(normalized);
    }

    [GeneratedRegex("^[A-Z0-9]+$", RegexOptions.CultureInvariant)]
    private static partial Regex LicensePlateRegex();
}
