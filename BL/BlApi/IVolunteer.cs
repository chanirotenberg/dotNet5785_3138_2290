
namespace BlApi;
/// <summary>
/// Interface representing volunteer services in the business logic layer.
/// </summary>
public interface IVolunteer : IObservable //stage 5
{
    /// <summary>
    /// Logs a user into the system and returns their role.
    /// </summary>
    /// <param name="username">The username of the volunteer.</param>
    /// <param name="password">The password of the volunteer.</param>
    /// <returns>The role of the user if login is successful.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if credentials are invalid.</exception>
    public BO.Jobs Login(string username, string password);

    /// <summary>
    /// Retrieves a list of volunteers filtered by activity status and sorted by a given field.
    /// </summary>
    /// <param name="isActive">Filter for active/inactive volunteers. Pass null for all.</param>
    /// <param name="sortBy">Field to sort by. Pass null to sort by ID.</param>
    /// <returns>A collection of volunteers in list view.</returns>
    IEnumerable<BO.VolunteerInList> GetVolunteerList(bool? isActive = null, BO.VolunteerSortField? sortBy = null);

    /// <summary>
    /// Retrieves details of a specific volunteer by ID.
    /// </summary>
    /// <param name="id">The ID of the volunteer.</param>
    /// <returns>A volunteer object with all relevant details.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the volunteer is not found.</exception>
    BO.Volunteer GetVolunteerDetails(int id);

    /// <summary>
    /// Updates an existing volunteer's information.
    /// </summary>
    /// <param name="id">The ID of the volunteer to update.</param>
    /// <param name="volunteer">The updated volunteer data.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the volunteer is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authorized to update.</exception>
    void UpdateVolunteer(int id, BO.Volunteer volunteer);

    /// <summary>
    /// Deletes a volunteer by ID.
    /// </summary>
    /// <param name="id">The ID of the volunteer to delete.</param>
    /// <exception cref="InvalidOperationException">Thrown if the volunteer cannot be deleted due to active assignments.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the volunteer is not found.</exception>
    void DeleteVolunteer(int id);

    /// <summary>
    /// Adds a new volunteer to the system.
    /// </summary>
    /// <param name="volunteer">The volunteer object to add.</param>
    /// <exception cref="InvalidOperationException">Thrown if the volunteer already exists.</exception>
    void CreateVolunteer(BO.Volunteer volunteer);
}
