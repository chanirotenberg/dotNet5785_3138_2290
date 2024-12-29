
namespace BO;
/// <summary>
/// Enum representing different types of call resolution.
/// </summary>
public enum EndType
{
    cared,
    selfCancellation,
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
    InTreatment,
    InRiskTreatment
}


