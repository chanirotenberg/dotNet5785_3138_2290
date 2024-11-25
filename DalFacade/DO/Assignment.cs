// Module Assignment.cs

namespace DO;

/// <summary>
/// Represents an assignment entity, linking a volunteer to a call with details about the timing and status of the assignment.
/// </summary>
/// <param name="Id">The unique identifier for the assignment.</param>
/// <param name="CallId">The unique identifier of the associated call.</param>
/// <param name="VolunteerId">The unique identifier of the assigned volunteer.</param>
/// <param name="EntryTime">The date and time when the assignment started.</param>
/// <param name="ActualEndTime">The date and time when the assignment ended, or null if it hasn't ended yet.</param>
/// <param name="EndType">The type of ending for the assignment, or null if not applicable.</param>
public record Assignment
(
    int CallId,
    int VolunteerId,
    DateTime EntryTime,
    DateTime? ActualEndTime,
    EndType? EndType
)
{
    public int Id { get; set; }
    /// <summary>
    /// Default constructor for the Assignment record, initializing default values.
    /// </summary>
    public Assignment() : this( 0, 0, DateTime.Now, null, null) { }
}
