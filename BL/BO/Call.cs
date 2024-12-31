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
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Type of the call.
    /// </summary>
    public CallType CallType { get; set; }

    /// <summary>
    /// Verbal description of the call.
    /// Nullable.
    /// </summary>
    public string? VerbalDescription { get; set; }

    /// <summary>
    /// Full address of the call.
    /// Must be a valid address.
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// Latitude of the call's location.
    /// Computed based on the address.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude of the call's location.
    /// Computed based on the address.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// The opening time of the call.
    /// Automatically set when the call is created.
    /// Cannot be updated.
    /// </summary>
    public DateTime OpeningTime { get; init; }

    /// <summary>
    /// Maximum allowed time to complete the call.
    /// Nullable.
    /// Must be validated during creation or update.
    /// </summary>
    public DateTime? MaximumTime { get; set; }

    /// <summary>
    /// Current status of the call.
    /// Computed based on the assignments and current system time.
    /// </summary>
    public CallStatus Status { get; set; }

    /// <summary>
    /// List of assignments related to the call, in the past or present.
    /// Nullable.
    /// </summary>
    public List<BO.CallAssignInList>? Assignments { get; set; }
    public override string ToString() => this.ToStringProperty();
}
