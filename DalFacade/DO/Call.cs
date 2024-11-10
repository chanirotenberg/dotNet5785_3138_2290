// Module Call.cs
namespace DO;
/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="CallType"></param>
/// <param name="VerbalDescription"></param>
/// <param name="address"></param>
/// <param name="Latitude"></param>
/// <param name="Longitude"></param>
/// <param name="OpeningTime"></param>
/// <param name="MaximumTime"></param>
public record Call
(
    int Id,
    CallType CallType,
    string VerbalDescription,
    string address,
    double Latitude,
    double Longitude,
    DateTime OpeningTime,
    DateTime MaximumTime
);
