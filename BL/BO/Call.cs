using Helpers;

namespace BO;

/// <summary>
/// Represents a call entity that includes details about a call and its assignments.
/// Used for view, add, update, and delete operations in the system.
/// </summary>
public class Call
{
    /// <summary>
    /// Unique identifier of the call.
    /// Auto-generated upon creation.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Type of the call (e.g., Transport, PickUp, etc.).
    /// Determines how the call will be handled.
    /// </summary>
    public CallType CallType { get; set; }

    /// <summary>
    /// Verbal description of the call.
    /// Optional field that provides additional details.
    /// Nullable.
    /// </summary>
    public string? VerbalDescription { get; set; }

    /// <summary>
    /// Full address of the call.
    /// Must be a valid address.
    /// Used to compute latitude and longitude.
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// Latitude of the call's location.
    /// Automatically computed based on the address.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude of the call's location.
    /// Automatically computed based on the address.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// The time when the call was created.
    /// Automatically set upon call creation.
    /// Cannot be updated once set.
    /// </summary>
    public DateTime OpeningTime { get; init; }

    /// <summary>
    /// Maximum allowed time to complete the call.
    /// Nullable field, meaning some calls may not have a defined deadline.
    /// Must be validated during creation or update.
    /// </summary>
    public DateTime? MaximumTime { get; set; }

    /// <summary>
    /// Current status of the call.
    /// Computed dynamically based on the assignments and current system time.
    /// </summary>
    public CallStatus Status { get; set; }

    /// <summary>
    /// List of assignments related to the call, in the past or present.
    /// Nullable.
    /// Used to track which volunteers handled the call.
    /// </summary>
    public List<BO.CallAssignInList>? Assignments { get; set; }

    /// <summary>
    /// Returns a formatted string representation of the call details.
    /// Includes call information such as ID, type, address, status, and assignments.
    /// If there are assignments, they will be listed; otherwise, "No assignments" will be displayed.
    /// </summary>
    /// <returns>Formatted string representing the call.</returns>
    public override string ToString()
    {
        string assignmentsString = Assignments != null && Assignments.Any()
            ? string.Join("\n", Assignments.Select(a => a.ToString()))
            : "No assignments";

        return $"Call ID: {Id}, Type: {CallType}, Address: {Address},\n" +
               $"Latitude: {Latitude}, Longitude: {Longitude},\n" +
               $"Opening Time: {OpeningTime}, Max Time: {MaximumTime},\n" +
               $"Status: {Status},\nAssignments:\n{assignmentsString}";
    }
}
