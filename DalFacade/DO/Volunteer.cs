namespace DO;

/// <summary>
/// Represents a volunteer with various details and preferences.
/// </summary>
/// <param name="Id">The unique identifier for the volunteer.</param>
/// <param name="Name">The name of the volunteer.</param>
/// <param name="Phone">The phone number of the volunteer.</param>
/// <param name="Email">The email address of the volunteer.</param>
/// <param name="Password">The password for the volunteer (optional).</param>
/// <param name="Address">The address of the volunteer (optional).</param>
/// <param name="Latitude">The latitude of the volunteer's location (optional).</param>
/// <param name="Longitude">The longitude of the volunteer's location (optional).</param>
/// <param name="Jobs">The job role of the volunteer, defaulting to Worker.</param>
/// <param name="active">Indicates whether the volunteer is active, defaulting to false.</param>
/// <param name="MaxDistance">The maximum distance the volunteer is willing to work (optional).</param>
/// <param name="DistanceType">The type of distance calculation used, defaulting to AirDistance.</param>
public record Volunteer
(
    int Id,
    string Name,
    string Phone,
    string Email,
    string? Password = null,
    string? Address = null,
    double? Latitude = null,
    double? Longitude = null,
    Jobs Jobs = Jobs.Worker,
    bool active = false,
    double? MaxDistance = null,
    DistanceType DistanceType = DistanceType.AirDistance
)
{
    /// <summary>
    /// Default constructor for creating an empty volunteer record (used for stage 4).
    /// </summary>
    public Volunteer() : this(0, "", "", "") { } // empty ctor for stage 4
}
