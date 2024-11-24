

namespace DO;
/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Phone"></param>
/// <param name="Email"></param>
/// <param name="Password"></param>
/// <param name="Address"></param>
/// <param name="Latitude"></param>
/// <param name="Longitude"></param>
/// <param name="Jobs"></param>
/// <param name="active"></param>
/// <param name="MaxDistance"></param>
/// <param name="DistanceType"></param>
public record Volunteer
(
    int Id,
    string Name,
    string Phone,
    string Email,   
    string? Password=null,
    string? Address= null,
    double? Latitude= null,
    double? Longitude= null,
    Jobs Jobs = Jobs.Worker,    
    bool active=false,
    double? MaxDistance = null,
    DistanceType DistanceType=DistanceType.AirDistance
)
{
    public Volunteer() : this(0,"","","") { } // empty ctor for stage 4
}

