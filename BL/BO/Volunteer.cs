namespace BO;
using Helpers;
/// <summary>
/// Represents a volunteer in the system.
/// </summary>
public class Volunteer
{
    /// <summary>
    /// The unique ID of the volunteer.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The name of the volunteer.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The phone number of the volunteer.
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// The email address of the volunteer.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// The password of the volunteer (optional).
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// The address of the volunteer (optional).
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// The latitude coordinate of the volunteer’s location (optional).
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// The longitude coordinate of the volunteer’s location (optional).
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// The job or role assigned to the volunteer.
    /// </summary>
    public Jobs Jobs { get; set; }

    /// <summary>
    /// Indicates whether the volunteer is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The maximum distance (in kilometers) the volunteer is willing to travel for calls (optional).
    /// </summary>
    public double? MaxDistance { get; set; }

    /// <summary>
    /// The type of distance measure used for the volunteer's location.
    /// </summary>
    public DistanceType DistanceType { get; set; }

    /// <summary>
    /// The total number of calls the volunteer has handled.
    /// </summary>
    public int SumOfCalls { get; set; }

    /// <summary>
    /// The total number of calls the volunteer has canceled.
    /// </summary>
    public int SumOfCancellation { get; set; }

    /// <summary>
    /// The total number of calls that expired without being treated by the volunteer.
    /// </summary>
    public int SumOfExpiredCalls { get; set; }

    /// <summary>
    /// The call currently in progress with the volunteer (if any).
    /// </summary>
    public BO.CallInProgress? CallInProgress { get; set; }
    public override string ToString() => this.ToStringProperty();
}