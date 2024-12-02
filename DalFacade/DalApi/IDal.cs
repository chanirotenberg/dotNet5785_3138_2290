namespace DalApi;

public interface IDal
{
    /// <summary>
    /// Gets the assignment interface for CRUD operations on assignments.
    /// </summary>
    IAssignment Assignment { get; }

    /// <summary>
    /// Gets the call interface for CRUD operations on calls.
    /// </summary>
    ICall Call { get; }

    /// <summary>
    /// Gets the volunteer interface for CRUD operations on volunteers.
    /// </summary>
    IVolunteer Volunteer { get; }

    /// <summary>
    /// Gets the config interface for CRUD operations on configuration settings.
    /// </summary>
    IConfig Config { get; }

    /// <summary>
    /// Resets the database, clearing all stored data.
    /// </summary>
    void ResetDB();
}
