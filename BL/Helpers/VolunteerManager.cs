
using System.Web;
using System.Text.Json;

namespace Helpers;

internal static class VolunteerManager
{
    private const string LocationIqApiKey = "pk.e7a4b1005a41f28c0d56501fccf80b77"; // Replace with your LocationIQ API key

    /// <summary>
    /// Gets the coordinates (latitude and longitude) for a given address using LocationIQ API.
    /// </summary>
    /// <param name="address">The address to retrieve coordinates for.</param>
    /// <returns>A tuple containing latitude and longitude.</returns>
    private static (double Latitude, double Longitude) GetCoordinates(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty.");

        using var client = new HttpClient();
        var url = $"https://us1.locationiq.com/v1/search.php?key={LocationIqApiKey}&q={HttpUtility.UrlEncode(address)}&format=json";

        try
        {
            var response = client.GetAsync(url).Result; // Synchronous HTTP request
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to retrieve coordinates. Status: {response.StatusCode}");

            var json = response.Content.ReadAsStringAsync().Result;
            var results = JsonDocument.Parse(json).RootElement.EnumerateArray();
            if (!results.MoveNext())
                throw new Exception("No results found for the given address.");

            var location = results.Current;
            double latitude = double.Parse(location.GetProperty("lat").GetString());
            double longitude = double.Parse(location.GetProperty("lon").GetString());
            return (latitude, longitude);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to retrieve coordinates for address: {address}. Details: {ex.Message}");
        }
    }

    /// <summary>
    /// Calculates the air distance (in kilometers) between two addresses.
    /// </summary>
    /// <param name="address1">The first address.</param>
    /// <param name="address2">The second address.</param>
    /// <returns>The air distance in kilometers.</returns>
    public static double CalculateAirDistance(string address1, string address2)
    {
        try
        {
            var (lat1, lon1) = GetCoordinates(address1);
            var (lat2, lon2) = GetCoordinates(address2);

            const double R = 6371; // Earth's radius in kilometers
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to calculate air distance between addresses. Details: {ex.Message}");
        }
    }

    public static void ValidateVolunteer(BO.Volunteer volunteer)
    {
        // Validate Name
        if (string.IsNullOrWhiteSpace(volunteer.Name) || volunteer.Name.Length < 2)
            throw new BO.BlValidationException("Name must be at least 2 characters long.");

        // Validate Email
        if (!volunteer.Email.Contains("@") || !volunteer.Email.Contains("."))
            throw new BO.BlValidationException("Invalid email format.");

        // Validate Latitude and Longitude
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

        // Validate Phone (must be numeric and 10 digits)
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
                var (latitude, longitude) = GetCoordinates(volunteer.Address);
                volunteer.Latitude = latitude;
                volunteer.Longitude = longitude;
            }
            catch (Exception ex)
            {
                throw new BO.BlValidationException($"Invalid address: {volunteer.Address}. Details: {ex.Message}");
            }
        }
    }

    // Helper function to validate Israeli ID
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

    // Helper function to check if a string is numeric
    private static bool IsNumeric(string value)
    {
        return value.All(char.IsDigit);
    }

    // Helper function to check if a value is a positive integer
    private static bool IsPositiveInteger(string value)
    {
        return int.TryParse(value, out int result) && result > 0;
    }


    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">Degrees to convert.</param>
    /// <returns>Radians.</returns>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}

