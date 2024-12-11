// Module configuration.cs
namespace Dal;

/// <summary>
/// Represents a configuration class for managing IDs, system clock, and risk range.
/// </summary>
/// <param name="NextCallId">The next available call ID, incremented each time a new call is created.</param>
/// <param name="NextAssignmentId">The next available assignment ID, incremented each time a new assignment is created.</param>
/// <param name="Clock">The system clock representing the current date and time.</param>
/// <param name="RiskRange">The risk range, representing the time span of risk (e.g., 1 hour).</param>
internal static class Config
{
    /// <summary>
    /// The starting value for the Call ID.
    /// </summary>
    internal const int startCallId = 1;
    private static int nextCallId = startCallId;

    /// <summary>
    /// Gets the next available Call ID and increments it for the next call.
    /// </summary>
    internal static int NextCallId { get => nextCallId++; }

    /// <summary>
    /// The starting value for the Assignment ID.
    /// </summary>
    internal const int startAssignmentId = 1;

    private static int nextAssignmentId = startAssignmentId;

    /// <summary>
    /// Gets the next available Assignment ID and increments it for the next assignment.
    /// </summary>
    internal static int NextAssignmentId { get => nextAssignmentId++; }

    /// <summary>
    /// The system clock representing the current date and time. Defaults to the current time when the application starts.
    /// </summary>
    internal static DateTime Clock { get; set; } = DateTime.Now;

    /// <summary>
    /// The risk range represented as a TimeSpan, defaults to 1 hour.
    /// </summary>
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Resets the configuration values to their default settings.
    /// </summary>
    internal static void Reset()
    {
        nextAssignmentId = startAssignmentId;
        nextCallId = startCallId;
        Clock = DateTime.Now;
        RiskRange = TimeSpan.FromHours(1);
    }
}
