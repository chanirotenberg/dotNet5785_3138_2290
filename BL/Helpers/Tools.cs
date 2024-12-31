
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
}
