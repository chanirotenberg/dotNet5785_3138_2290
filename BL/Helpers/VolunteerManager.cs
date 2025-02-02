using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Text.Json;

namespace Helpers;

/// <summary>
/// Manages volunteer-related operations, including validation and address-based calculations.
/// </summary>
internal static class VolunteerManager
{
    /// <summary>
    /// Validates a volunteer object to ensure correct data input.
    /// </summary>
    /// <param name="volunteer">The volunteer object to validate.</param>
    public static void ValidateVolunteer(BO.Volunteer volunteer,string? oldPassword = null)
    {
        // Validate Name
        if (string.IsNullOrWhiteSpace(volunteer.Name) || volunteer.Name.Length < 2)
            throw new BO.BlValidationException("Name must be at least 2 characters long.");

        // Validate Email format
        if (!volunteer.Email.Contains("@") || !volunteer.Email.Contains("."))
            throw new BO.BlValidationException("Invalid email format.");

        if (volunteer.Password != oldPassword)
        {
            // Validate Password Strength
            if (!IsStrongPassword(volunteer.Password, oldPassword))
                throw new BO.BlValidationException("Password must be at least 8 characters long, include an uppercase letter, a lowercase letter, a number, and a special character.");
            // Encrypt the password before storing it
            volunteer.Password = EncryptPassword(volunteer.Password);
        }
            

        // Validate Latitude and Longitude (if provided)
        if (volunteer.Latitude.HasValue && (volunteer.Latitude < -90 || volunteer.Latitude > 90))
            throw new BO.BlValidationException("Latitude must be between -90 and 90 degrees.");
        if (volunteer.Longitude.HasValue && (volunteer.Longitude < -180 || volunteer.Longitude > 180))
            throw new BO.BlValidationException("Longitude must be between -180 and 180 degrees.");

        // Validate ID (must be numeric and valid)
        if (!IsValidIsraeliId(volunteer.Id))
            throw new BO.BlValidationException("Invalid Israeli ID.");

        // Validate MaxDistance (must be a positive number if provided)
        if (volunteer.MaxDistance.HasValue && volunteer.MaxDistance <= 0)
            throw new BO.BlValidationException("MaxDistance must be a positive number.");

        // Validate Phone number (must be numeric and 10 digits)
        if (!IsNumeric(volunteer.Phone) || volunteer.Phone.Length != 10)
            throw new BO.BlValidationException("Phone number must be numeric and 10 digits long.");

        // Validate other numeric fields
        if (!IsPositiveInteger(volunteer.Id.ToString()))
            throw new BO.BlValidationException("ID must be a positive integer.");

        // Validate Address (if provided)
        if (!string.IsNullOrWhiteSpace(volunteer.Address))
        {
            try
            {
                var (latitude, longitude) = Tools.GetCoordinates(volunteer.Address);
                volunteer.Latitude = latitude;
                volunteer.Longitude = longitude;
            }
            catch (Exception ex)
            {
                throw new BO.BlValidationException($"Invalid address: {volunteer.Address}. Details: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Checks if a password is strong.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns>True if the password is strong, otherwise false.</returns>
    private static bool IsStrongPassword(string password,string oldPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }    

    /// <summary>
    /// Encrypts a password using SHA-256.
    /// </summary>
    /// <param name="password">The password to encrypt.</param>
    /// <returns>The encrypted password as a hexadecimal string.</returns>
    public static string EncryptPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }

    /// <summary>
    /// Checks if an Israeli ID is valid using checksum validation.
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <returns>True if the ID is valid, otherwise false.</returns>
    private static bool IsValidIsraeliId(int id)
    {
        var idStr = id.ToString("D9"); // Pad to 9 digits
        int sum = 0;
        for (int i = 0; i < idStr.Length; i++)
        {
            int digit = int.Parse(idStr[i].ToString());
            if (i % 2 == 1) digit *= 2;
            sum += digit > 9 ? digit - 9 : digit;
        }
        return sum % 10 == 0;
    }

    /// <summary>
    /// Checks if a given string is numeric.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string is numeric, otherwise false.</returns>
    private static bool IsNumeric(string value)
    {
        return value.All(char.IsDigit);
    }

    /// <summary>
    /// Checks if a given string represents a positive integer.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string is a positive integer, otherwise false.</returns>
    private static bool IsPositiveInteger(string value)
    {
        return int.TryParse(value, out int result) && result > 0;
    }
}
