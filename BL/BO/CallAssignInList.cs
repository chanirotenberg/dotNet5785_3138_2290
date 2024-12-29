namespace BO;

/// <summary>
/// Represents a call assignment in the list (for view only).
/// </summary>
public class CallAssignInList
{
    /// <summary>
    /// The volunteer's ID.
    /// </summary>
    public int? VolunteerId { get; init; }

    /// <summary>
    /// The name of the volunteer.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The time the call was assigned to the volunteer.
    /// </summary>
    public DateTime EntryTime { get; init; }

    /// <summary>
    /// The actual time the treatment was completed.
    /// </summary>
    public DateTime? EndTime { get; init; }

    /// <summary>
    /// The type of treatment completion (e.g., Completed, Canceled due to expiration).
    /// </summary>
    public EndType? EndType { get; init; }

}
