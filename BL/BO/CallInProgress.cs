using Helpers;

namespace BO;

/// <summary>
/// Represents a call currently being handled by a volunteer.
/// </summary>
public class CallInProgress
{
    /// <summary>
    /// The unique ID of the assignment (internal use, not displayed).
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The unique ID of the call.
    /// </summary>
    public int CallId { get; init; }

    /// <summary>
    /// The type of call (Transport, PickUp, etc.).
    /// </summary>
    public CallType CallType { get; init; }

    /// <summary>
    /// A verbal description of the call (optional).
    /// </summary>
    public string? VerbalDescription { get; init; }

    /// <summary>
    /// The full address of the call.
    /// </summary>
    public string Address { get; init; }

    /// <summary>
    /// The time the call was opened.
    /// </summary>
    public DateTime OpeningTime { get; init; }

    /// <summary>
    /// The maximum time for the call to be completed (optional).
    /// </summary>
    public DateTime? MaximumTime { get; init; }

    /// <summary>
    /// The time the volunteer started handling the call.
    /// </summary>
    public DateTime EntryTime { get; init; }

    /// <summary>
    /// The distance from the volunteer to the call location.
    /// </summary>
    public double Distance { get; init; }

    /// <summary>
    /// The current status of the call (InProgress, AtRisk, etc.).
    /// </summary>
    public CallStatus Status { get; init; }
    public override string ToString() => this.ToStringProperty();
}
