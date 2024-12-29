namespace BO;

/// <summary>
/// Represents an open call in the list view, available for volunteer selection.
/// </summary>
public class OpenCallInList
{
    /// <summary>
    /// The unique ID of the open call.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The type of the call.
    /// </summary>
    public CallType CallType { get; init; }

    /// <summary>
    /// A verbal description of the call (nullable).
    /// </summary>
    public string? VerbalDescription { get; init; }

    /// <summary>
    /// The full address of the call.
    /// </summary>
    public string Address { get; init; }

    /// <summary>
    /// The opening time of the call.
    /// </summary>
    public DateTime OpeningTime { get; init; }

    /// <summary>
    /// The maximum allowed time to complete the call (nullable).
    /// </summary>
    public DateTime? MaximumTime { get; init; }

    /// <summary>
    /// The distance from the volunteer to the call location.
    /// </summary>
    public double DistanceFromVolunteer { get; init; }
}
