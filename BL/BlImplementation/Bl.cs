namespace BlImplementation;
using BlApi;

/// <summary>
/// Main business logic layer class that implements the IBl interface.
/// Provides access to all logical entities: Volunteers, Calls, and Admin functionalities.
/// </summary>
internal class Bl : IBl
{
    /// <summary>
    /// Provides access to volunteer-related operations.
    /// </summary>
    public IVolunteer Volunteer { get; } = new VolunteerImplementation();

    /// <summary>
    /// Provides access to call-related operations.
    /// </summary>
    public ICall Call { get; } = new CallImplementation();

    /// <summary>
    /// Provides access to administrative operations.
    /// </summary>
    public IAdmin Admin { get; } = new AdminImplementation();
}
