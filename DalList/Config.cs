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

public class Config
{
    private static int NextCallId = 1;
    private static int NextAssignmentId = 1;
    private static DateTime Clock;
    private static TimeSpan RiskRange;


    // קבלת מספר רץ חדש עבור סוג ישות מסוים
    public static int GetNextId(string entityType)
    {
        return entityType switch
        {
            "Call" => NextCallId++,
            "Assignment" => NextAssignmentId++,
            _ => throw new ArgumentException("Unknown entity type")
        };
    }

}


