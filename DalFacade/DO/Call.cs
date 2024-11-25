// Module Call.cs

namespace DO;

/// <summary>
/// Represents a call entity, detailing information about a call for assistance.
/// </summary>
/// <param name="Id">The unique identifier for the call.</param>
/// <param name="CallType">The type of the call (e.g., transport, medical).</param>
/// <param name="VerbalDescription">A description of the call, providing additional details. Nullable.</param>
/// <param name="address">The address where the call is located.</param>
/// <param name="Latitude">The latitude of the call's location.</param>
/// <param name="Longitude">The longitude of the call's location.</param>
/// <param name="OpeningTime">The time when the call was opened.</param>
/// <param name="MaximumTime">The maximum allowable time to handle the call, or null if not applicable.</param>
public record Call
(
    CallType CallType,
    string? VerbalDescription,
    string Address,
    double Latitude,
    double Longitude,
    DateTime OpeningTime,
    DateTime? MaximumTime
)
{
    public int Id {  get; set; }
    /// <summary>
    /// Default constructor for the Call record, initializing default values.
    /// </summary>
    public Call() : this( CallType.Transport, null, "", 0, 0, DateTime.Now, null) { }
}
