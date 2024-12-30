using Helpers;

namespace BO;

/// <summary>
/// Represents a volunteer in the list view.
/// </summary>
public class VolunteerInList
{
    /// <summary>
    /// The unique ID of the volunteer.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The full name of the volunteer.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Indicates if the volunteer is currently active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// The total number of calls handled by the volunteer.
    /// </summary>
    public int TotalHandledCalls { get; init; }

    /// <summary>
    /// The total number of calls canceled by the volunteer.
    /// </summary>
    public int TotalCanceledCalls { get; init; }

    /// <summary>
    /// The total number of expired calls the volunteer was responsible for.
    /// </summary>
    public int TotalExpiredCalls { get; init; }

    /// <summary>
    /// The ID of the call currently being handled by the volunteer (if any).
    /// </summary>
    public int? CurrentCallId { get; init; }

    /// <summary>
    /// The type of call currently being handled by the volunteer.
    /// </summary>
    public CallType CallType { get; init; } = CallType.None;
    public override string ToString() => this.ToStringProperty();
}
