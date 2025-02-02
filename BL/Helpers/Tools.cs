
using System.Text.Json;
using System.Web;

namespace Helpers;

internal static class Tools
{
    /// <summary>
    /// Generates a string with all properties and their values of the given object.
    /// </summary>
    /// <typeparam name="T">The type of the object being processed.</typeparam>
    /// <param name="t">The object to process.</param>
    /// <returns>A string representing all properties and their values.</returns>
    public static string ToStringProperty<T>(this T t)
    {
        if (t == null)
            return "Object is null";

        // Using reflection to get all public properties of the object
        var properties = t.GetType().GetProperties();

        // Build the output string by iterating over the properties
        var result = new System.Text.StringBuilder();
        result.AppendLine($"Type: {t.GetType().Name}");

        foreach (var prop in properties)
        {
            var value = prop.GetValue(t) ?? "null";
            result.AppendLine($"{prop.Name}: {value}");
        }

        return result.ToString();
    }

    /// <summary>
    /// API key for the LocationIQ mapping service.
    /// </summary>
    private const string LocationIqApiKey = "pk.e7a4b1005a41f28c0d56501fccf80b77";

    /// <summary>
    /// Cache for storing address coordinates to reduce API calls.
    /// </summary>
    private static readonly Dictionary<string, (double Latitude, double Longitude)> _addressCache = new();

    /// <summary>
    /// Retrieves the latitude and longitude coordinates for a given address.
    /// </summary>
    /// <param name="address">The address to fetch coordinates for.</param>
    /// <returns>A tuple containing latitude and longitude.</returns>
    public static (double Latitude, double Longitude) GetCoordinates(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new BO.BlValidationException("Address cannot be null or empty.");

        // Check if the coordinates are cached
        if (_addressCache.TryGetValue(address, out var cachedCoordinates))
            return cachedCoordinates;

        using var client = new HttpClient();
        var url = $"https://us1.locationiq.com/v1/search.php?key={LocationIqApiKey}&q={HttpUtility.UrlEncode(address)}&format=json";

        for (int i = 0; i < 3; i++) // Try up to 3 times if TooManyRequests is received
        {
            try
            {
                var response = client.GetAsync(url).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    if (i < 2) // If not the last attempt, wait and retry
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    throw new BO.BlException("Too many requests to the mapping service. Please try again later.");
                }

                if (!response.IsSuccessStatusCode)
                    throw new BO.BlException($"Failed to retrieve coordinates. Status: {response.StatusCode}");

                var json = response.Content.ReadAsStringAsync().Result;
                var results = JsonDocument.Parse(json).RootElement.EnumerateArray();
                if (!results.MoveNext())
                    throw new BO.BlException("No results found for the given address.");

                var location = results.Current;
                double latitude = double.Parse(location.GetProperty("lat").GetString());
                double longitude = double.Parse(location.GetProperty("lon").GetString());

                // Store the address in the cache
                _addressCache[address] = (latitude, longitude);
                return (latitude, longitude);
            }
            catch (Exception ex)
            {
                throw new BO.BlException($"Failed to retrieve coordinates for address: {address}. Details: {ex.Message}");
            }
        }

        throw new BO.BlException($"Failed to retrieve coordinates for address: {address}. Too many attempts.");
    }
}
