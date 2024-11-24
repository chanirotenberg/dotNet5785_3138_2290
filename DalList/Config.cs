// Module configuration.cs
namespace Dal;
/// <summary>
/// 
/// </summary>
/// <param name="NextCallId"></param>
/// <param name="NextAssignmentId"></param>
/// <param name="Clock"></param>
/// <param name="RiskRange"></param>
//public record Config
//{
//    static int NextCallId;
//    int NextAssignmentId;
//    DateTime Clock;
//    TimeSpan RiskRange;
//}


internal static class Config:
{
    internal const int startCallId = 1;
    private static int nextCallId = startCallId;
    internal static int NextCallId { get => nextCallId++; }
    

    internal const int startAssignmentId = 1;
    private static int nextAssignmentId = startAssignmentId;
    internal static int NextAssignmentId { get => nextAssignmentId++; }

    internal static DateTime Clock { get; set; } = DateTime.Now;

    internal static TimeSpan RiskRange = TimeSpan.Zero;

    internal static void Reset()
    {
        nextAssignmentId = startAssignmentId;
        nextCallId = startCallId;
        
        Clock = DateTime.Now;

        RiskRange = TimeSpan.Zero;
    }

}


