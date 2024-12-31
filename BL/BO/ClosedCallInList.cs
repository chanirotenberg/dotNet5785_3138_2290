using Helpers;

namespace BO;

/// <summary>
/// Represents a closed call in the list view.
/// </summary>
public class ClosedCallInList
{
    /// <summary>
    /// The unique ID of the closed call.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The type of the call.
    /// </summary>
    public CallType CallType { get; init; }

    /// <summary>
    /// The full address of the call.
    /// </summary>
    public string Address { get; init; }

    /// <summary>
    /// The opening time of the call.
    /// </summary>
    public DateTime OpeningTime { get; init; }

    /// <summary>
    /// The time the volunteer started handling the call.
    /// </summary>
    public DateTime EntryTime { get; init; }

    /// <summary>
    /// The actual end time of the call (nullable).
    /// </summary>
    public DateTime? EndTime { get; init; }

    /// <summary>
    /// The type of end status for the call (nullable).
    /// </summary>
    public EndType? EndType { get; init; }
    public override string ToString() => this.ToStringProperty();
}
