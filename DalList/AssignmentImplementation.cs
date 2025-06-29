namespace Dal;
using DalApi;
using DO;
using System.Runtime.CompilerServices;

/// <summary>
/// Implementation of assignment-related operations.
/// </summary>
internal class AssignmentImplementation : IAssignment
{
    /// <summary>
    /// Creates a new assignment.
    /// </summary>
    /// <param name="item">The assignment to be created.</param>
    /// <exception cref="DalAlreadyExistsException">Thrown if an assignment with the given ID already exists.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Assignment item)
    {
        int newId = Config.NextAssignmentId;
        Assignment newAssignments = item with { Id = newId };
        DataSource.Assignments.Add(newAssignments);
    }

    /// <summary>
    /// Deletes an assignment by its ID.
    /// </summary>
    /// <param name="id">The ID of the assignment to be deleted.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the assignment with the given ID does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        Assignment? assignment = DataSource.Assignments.FirstOrDefault(a => a.Id == id) ?? throw new DalDoesNotExistException($"Assignment Object with {id} doesn't exist");
        DataSource.Assignments.Remove(assignment);
    }

    /// <summary>
    /// Deletes all assignments.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        DataSource.Assignments.Clear();
    }

    /// <summary>
    /// Reads (retrieves) an assignment by its ID.
    /// </summary>
    /// <param name="id">The ID of the assignment to be retrieved.</param>
    /// <returns>The assignment object if found; otherwise, null.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(int id)
    {
        return DataSource.Assignments.FirstOrDefault(a => a.Id == id);
    }

    /// <summary>
    /// Reads (retrieves) the first assignment that matches a specific filter.
    /// </summary>
    /// <param name="filter">The filter function to apply.</param>
    /// <returns>The assignment object if a match is found; otherwise, null.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(Func<Assignment, bool> filter)
    {
        return DataSource.Assignments.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all assignments.
    /// </summary>
    /// <returns>A list of all assignment objects.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public List<Assignment> ReadAll()
    {
        return new List<Assignment>(DataSource.Assignments);
    }

    /// <summary>
    /// Reads all assignments with an optional filter.
    /// </summary>
    /// <param name="filter">The optional filter function to apply.</param>
    /// <returns>An enumerable of assignments that match the filter, or all assignments if no filter is provided.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null) //stage 2
        => filter == null
            ? DataSource.Assignments.Select(item => item)
            : DataSource.Assignments.Where(filter);

    /// <summary>
    /// Updates an existing assignment with new values.
    /// </summary>
    /// <param name="item">The assignment object with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the assignment with the given ID does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Assignment item)
    {
        // Search for an existing assignment by ID
        Assignment? existingAssignment = DataSource.Assignments.FirstOrDefault(a => a.Id == item.Id) ?? throw new DalDoesNotExistException($"Assignment Object with id: {item.Id} doesn't exist");

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
