// Module Assignment.cs

namespace DO;
/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="CallId"></param>
/// <param name="VolunteerId"></param>
/// <param name="EntryTime"></param>
/// <param name="ActualEndTime"></param>
/// <param name="EndType"></param>
public record Assignment
(
    int Id,
    int CallId,
    int VolunteerId,
    DateTime EntryTime,
    DateTime? ActualEndTime,
    EndType? EndType
)
{
 public Assignment() : this(0, 0, 0, DateTime.Now, null, null) { }
}
