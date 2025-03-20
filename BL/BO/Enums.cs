
namespace BO;
/// <summary>
/// Enum representing different types of call resolution.
/// </summary>
public enum EndType
{
    Cared,
    SelfCancellation,
    AdministratorCancellation,
    ExpiredCancellation
}
/// <summary>
/// Enum representing different types of calls.
/// </summary>
public enum CallType
{
    Transport,
    PickUp,
    None
}

/// <summary>
/// Enum representing different job roles.
/// </summary>
public enum Jobs
{
    Administrator,
    Worker
}
/// <summary>
/// Enum representing different types of distances used for calculations.
/// </summary>
public enum DistanceType
{
    AirDistance,
    WalkingDistance,
    DrivingDistance
}

public enum CallStatus
{
    Open,
    InTreatment,
    Closed,
    Expired,
    InRiskTreatment,
    OpenInRisk
}

/// <summary>
/// Enum representing fields to sort volunteers by.
/// </summary>
public enum VolunteerSortField
{
    Id,
    Name,
    SumOfCalls,
    SumOfCancellation,
    SumOfExpiredCalls
}

/// <summary>
/// Represents fields by which calls can be sorted.
/// </summary>
public enum CallSortAndFilterField
{
    Id,
    CallType,
    OpeningTime,
    Status
}

/// <summary>
/// Represents units of time for advancing the system clock.
/// </summary>
public enum TimeUnit
{
    Minute,
    Hour,
    Day,
    Month,
    Year
}