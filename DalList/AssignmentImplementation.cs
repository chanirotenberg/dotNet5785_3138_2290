namespace Dal;
using DalApi;
using DO;

internal class AssignmentImplementation : IAssignment
{
    /// <summary>
    /// Creates a new assignment.
    /// </summary>
    /// <param name="item">The assignment to be created.</param>
    /// <exception cref="Exception">Thrown if an assignment with the given ID already exists.</exception>

    public void Create(Assignment item)
    {
        if (DataSource.Assignments.Any(a => a.Id == item.Id))
            throw new Exception("An assignment with this ID already exists.");
        int newId = Config.NextAssignmentId;
        Assignment newAssignments = item with { Id = newId};
        DataSource.Assignments.Add(newAssignments);
    }

    /// <summary>
    /// Deletes an assignment by its ID.
    /// </summary>
    /// <param name="id">The ID of the assignment to be deleted.</param>
    /// <exception cref="Exception">Thrown if the assignment with the given ID does not exist.</exception>
    public void Delete(int id)
    {
        Assignment? assignment = DataSource.Assignments.FirstOrDefault(a => a.Id == id) ?? throw new Exception($"Assignment Object with {id} doesn't exist");
        DataSource.Assignments.Remove(assignment);
    }

    /// <summary>
    /// Deletes all assignments.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Assignments.Clear();
    }

    /// <summary>
    /// Reads (retrieves) an assignment by its ID.
    /// </summary>
    /// <param name="id">The ID of the assignment to be retrieved.</param>
    /// <returns>The assignment object if found; otherwise, null.</returns>
    public Assignment? Read(int id)
    {
        return DataSource.Assignments.FirstOrDefault(a => a.Id == id);
    }

    /// <summary>
    /// Reads all assignments.
    /// </summary>
    /// <returns>A list of all assignment objects.</returns>
    public List<Assignment> ReadAll()
    {
        return new List<Assignment>(DataSource.Assignments);
    }

    /// <summary>
    /// Updates an existing assignment with new values.
    /// </summary>
    /// <param name="item">The assignment object with updated values.</param>
    /// <exception cref="Exception">Thrown if the assignment with the given ID does not exist.</exception>
    public void Update(Assignment item)
    {
        // Search for an existing assignment by ID
        Assignment? existingAssignment = DataSource.Assignments.FirstOrDefault(a => a.Id == item.Id) ?? throw new Exception($"Assignment Object with id: {item.Id} doesn't exist");

        // Create a new assignment object with the updated values
        Assignment? updatedAssignment = existingAssignment with
        {
            CallId = item.CallId,
            VolunteerId = item.VolunteerId,
            EntryTime = item.EntryTime,
            ActualEndTime = item.ActualEndTime,
            EndType = item.EndType
        };

        // Remove the old assignment
        DataSource.Assignments.Remove(existingAssignment);

        // Add the updated assignment
        DataSource.Assignments.Add(updatedAssignment);
    }
}
