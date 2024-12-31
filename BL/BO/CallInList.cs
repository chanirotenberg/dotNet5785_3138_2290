using Helpers;

namespace BO;

/// <summary>
/// Represents a call currently in the list (for view only).
/// </summary>
public class CallInList
{
    /// <summary>
    /// The ID of the assignment (internal use, not displayed).
    /// </summary>
    public int? Id { get; init; }

    /// <summary>
    /// The unique ID of the call.
    /// </summary>
    public int CallId { get; init; }

    /// <summary>
    /// The type of the call (e.g., Transport, PickUp, etc.).
    /// </summary>
    public CallType CallType { get; init; }

    /// <summary>
    /// The time when the call was opened.
    /// </summary>
    public DateTime OpeningTime { get; init; }

    /// <summary>
    /// The remaining time until the call should be completed.
    /// </summary>
    public TimeSpan? RemainingTime { get; init; }

    /// <summary>
    /// The name of the last volunteer assigned to the call (if applicable).
    /// </summary>
    public string? LastVolunteerName { get; init; }

    /// <summary>
    /// The total time taken to complete the treatment (if applicable).
    /// </summary>
    public TimeSpan? TotalTreatmentTime { get; init; }

    /// <summary>
    /// The current status of the call.
    /// </summary>
    public CallStatus Status { get; init; }

    /// <summary>
    /// The total number of assignments for this call.
    /// </summary>
    public int SumOfAssignments { get; init; }
    public override string ToString() => this.ToStringProperty();
}