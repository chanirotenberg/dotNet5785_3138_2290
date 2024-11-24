

namespace DalApi;

public interface IConfig
{
    DateTime Clock { get; set; }
    //int NextAssignmentId { get; } // הוספת המאפיין

    TimeSpan RiskRange { get; set; }
    void Reset();

}
