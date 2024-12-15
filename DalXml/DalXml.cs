namespace Dal;
using DalApi;

sealed public class DalXml: IDal
{
    /// <summary>
    /// Gets the IAssignment implementation for managing assignments.
    /// </summary>
    public IAssignment Assignment { get; } = new AssignmentImplementation();

    /// <summary>
    /// Gets the ICall implementation for managing calls.
    /// </summary>
    public ICall Call { get; } = new CallImplementation();

    /// <summary>
    /// Gets the IVolunteer implementation for managing volunteers.
    /// </summary>
    public IVolunteer Volunteer { get; } = new VolunteerImplementation();

    /// <summary>
    /// Gets the IConfig implementation for managing configuration settings.
    /// </summary>
    public IConfig Config { get; } = new ConfigImplementation();

    /// <summary>
    /// Resets the database by deleting all entities and resetting configuration.
    /// </summary>
    public void ResetDB()
    {
        Volunteer.DeleteAll();    // Deletes all volunteers
        Call.DeleteAll();         // Deletes all calls
        Assignment.DeleteAll();   // Deletes all assignments
        Config.Reset();           // Resets the configuration
    }
}
