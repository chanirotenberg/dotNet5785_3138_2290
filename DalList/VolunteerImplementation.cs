namespace Dal;
using DalApi;
using DO;

internal class VolunteerImplementation : IVolunteer
{
    /// <summary>
    /// Creates a new volunteer.
    /// </summary>
    /// <param name="item">The volunteer to be created.</param>
    /// <exception cref="Exception">Thrown if a volunteer with the given ID already exists.</exception>
    public void Create(Volunteer item)
    {
        if (this.Read(item.Id) is not null)
            throw new DalAlreadyExistsException($"Volunteer Object with {item.Id} already exists");
        DataSource.Volunteers.Add(item);
    }

    /// <summary>
    /// Reads (retrieves) a volunteer by ID.
    /// </summary>
    /// <param name="id">The ID of the volunteer to be retrieved.</param>
    /// <returns>The volunteer object if found; otherwise, null.</returns>
    public Volunteer? Read(int id)
    {
        return DataSource.Volunteers.FirstOrDefault(v => v.Id == id);
    }

    /// <summary>
    /// Reads all volunteers.
    /// </summary>
    /// <returns>A list of all volunteer objects.</returns>
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null) //stage 2
      => filter == null
          ? DataSource.Volunteers.Select(item => item)
          : DataSource.Volunteers.Where(filter);

    /// <summary>
    /// Deletes a volunteer by ID.
    /// </summary>
    /// <param name="id">The ID of the volunteer to be deleted.</param>
    /// <exception cref="Exception">Thrown if the volunteer with the given ID does not exist.</exception>
    public void Delete(int id)
    {
        Volunteer? existingVolunteer = DataSource.Volunteers.FirstOrDefault(v => v.Id == id) ?? throw new DalDoesNotExistException($"Volunteer Object with {id} doesn't exist");
        DataSource.Volunteers.Remove(existingVolunteer);
    }

    /// <summary>
    /// Deletes all volunteers.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Volunteers.Clear();
    }

    /// <summary>
    /// Updates an existing volunteer with new values.
    /// </summary>
    /// <param name="item">The volunteer object with updated values.</param>
    /// <exception cref="Exception">Thrown if the volunteer with the given ID does not exist.</exception>
    public void Update(Volunteer item)
    {
        Volunteer? existingVolunteer = DataSource.Volunteers.FirstOrDefault(v => v.Id == item.Id) ?? throw new DalDoesNotExistException($"Volunteer Object with {item.Id} doesn't exist");

        // Create a new volunteer record with updated values
        Volunteer updatedVolunteer = existingVolunteer with
        {
            Name = item.Name,
            Phone = item.Phone,
            Email = item.Email,
            Password = item.Password,
            Address = item.Address,
            Latitude = item.Latitude,
            Longitude = item.Longitude,
            Jobs = item.Jobs,
            active = item.active,
            MaxDistance = item.MaxDistance,
            DistanceType = item.DistanceType
        };

        // Remove the old volunteer and add the updated one
        DataSource.Volunteers.Remove(existingVolunteer);
        DataSource.Volunteers.Add(updatedVolunteer);
    }

    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        return DataSource.Volunteers.FirstOrDefault(filter);
    }
}
