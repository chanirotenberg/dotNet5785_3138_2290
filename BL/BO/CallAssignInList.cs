using Helpers;

namespace BO;

/// <summary>
/// Represents a call assignment in the list (for view only).
/// </summary>
public class CallAssignInList
{
    /// <summary>
    /// The volunteer's ID. Nullable in case no volunteer is assigned.
    /// </summary>
    public int? VolunteerId { get; init; }

    /// <summary>
    /// The name of the volunteer. Nullable in case the volunteer's name is not available.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The time when the call was assigned to the volunteer.
    /// This represents the moment the volunteer started handling the call.
    /// </summary>
    public DateTime EntryTime { get; init; }

    /// <summary>
    /// The actual time the treatment was completed.
    /// Nullable in case the treatment is still ongoing.
    /// </summary>
    public DateTime? EndTime { get; init; }

    /// <summary>
    /// The type of treatment completion.
    /// This field indicates whether the call was completed, self-canceled,
    /// canceled by an administrator, or expired.
    /// </summary>
    public EndType? EndType { get; init; }

    /// <summary>
    /// Converts the object properties into a readable string format.
    /// Displays the volunteer ID, name, entry time, end time, and end type.
    /// If the treatment is not finished, "Not finished" will be displayed for the end time.
    /// If the end type is null, "N/A" will be displayed.
    /// </summary>
    public override string ToString()
    {
        return $"Volunteer ID: {VolunteerId}, Name: {Name}, Entry: {EntryTime}, " +
               $"End: {(EndTime.HasValue ? EndTime.Value.ToString() : "Not finished")}, " +
               $"End Type: {(EndType.HasValue ? EndType.Value.ToString() : "N/A")}";
    }
}
