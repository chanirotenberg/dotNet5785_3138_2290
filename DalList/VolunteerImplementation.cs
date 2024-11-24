
namespace Dal;
using DalApi;
using DO;

public class VolunteerImplementation : IVolunteer
{
    private readonly List<Volunteer> volunteers = DataSource.Volunteers;

    // Create a new volunteer
    public void Create(Volunteer item)
    {
        if (volunteers.Any(v => v.Id == item.Id))
            throw new Exception("A volunteer with this ID already exists.");

        volunteers.Add(item);
    }

    // Delete a volunteer by ID
    public void Delete(int id)
    {
        Volunteer? existingVolunteer = volunteers.FirstOrDefault(v => v.Id == id);
        if (existingVolunteer == null)
            throw new Exception("The volunteer was not found.");

        volunteers.Remove(existingVolunteer);
    }

    // Delete all volunteers
    public void DeleteAll()
    {
        volunteers.Clear();
    }

    // Read a volunteer by ID
    public Volunteer? Read(int id)
    {
        return volunteers.FirstOrDefault(v => v.Id == id);
    }

    // Read all volunteers
    public List<Volunteer> ReadAll()
    {
        return volunteers;
    }

    // Update an existing volunteer
    public void Update(Volunteer item)
    {
        Volunteer? existingVolunteer = volunteers.FirstOrDefault(v => v.Id == item.Id);
        if (existingVolunteer == null)
            throw new Exception("The volunteer was not found.");

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
        volunteers.Remove(existingVolunteer);
        volunteers.Add(updatedVolunteer);
    }
}
