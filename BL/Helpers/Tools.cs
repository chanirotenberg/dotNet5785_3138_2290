using System.Text.Json;
using System.Web;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using BO;

namespace Helpers;

internal static class Tools
{
    /// <summary>
    /// Generates a string with all properties and their values of the given object.
    /// </summary>
    public static string ToStringProperty<T>(this T t)
    {
        if (t == null)
            return "Object is null";

        var properties = t.GetType().GetProperties();

        var result = new System.Text.StringBuilder();
        result.AppendLine($"Type: {t.GetType().Name}");

        foreach (var prop in properties)
        {
            var value = prop.GetValue(t) ?? "null";
            result.AppendLine($"{prop.Name}: {value}");
        }

        return result.ToString();
    }

    private const string LocationIqApiKey = "pk.e7a4b1005a41f28c0d56501fccf80b77";

    private static readonly Dictionary<string, (double Latitude, double Longitude)> _addressCache = new();

    /// <summary>
    /// Retrieves the latitude and longitude coordinates for a given address asynchronously.
    /// </summary>
    /// <param name="address">The address to fetch coordinates for.</param>
    /// <returns>A task that returns a tuple containing latitude and longitude.</returns>
    public static async Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new BO.BlValidationException("Address cannot be null or empty.");

        if (_addressCache.TryGetValue(address, out var cachedCoordinates))
            return cachedCoordinates;

        using var client = new HttpClient();
        var url = $"https://us1.locationiq.com/v1/search.php?key={LocationIqApiKey}&q={HttpUtility.UrlEncode(address)}&format=json";

        for (int i = 0; i < 3; i++) // Try up to 3 times if TooManyRequests is received
        {
            try
            {
                var response = await client.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    if (i < 2)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    throw new BO.BlException("Too many requests to the mapping service. Please try again later.");
                }

                if (!response.IsSuccessStatusCode)
                    throw new BO.BlException($"Failed to retrieve coordinates. Status: {response.StatusCode}");

                var json = await response.Content.ReadAsStringAsync();
                var results = JsonDocument.Parse(json).RootElement.EnumerateArray();
                if (!results.MoveNext())
                    throw new BO.BlException("No results found for the given address.");

                var location = results.Current;
                double latitude = double.Parse(location.GetProperty("lat").GetString());
                double longitude = double.Parse(location.GetProperty("lon").GetString());

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
    public static (double Latitude, double Longitude) GetCoordinatesSync(string address)
    {
        return Task.Run(() => GetCoordinatesAsync(address)).Result;
    }


}
