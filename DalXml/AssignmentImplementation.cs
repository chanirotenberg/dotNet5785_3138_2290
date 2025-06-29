namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

internal class AssignmentImplementation : IAssignment
{
    /// <summary>
    /// Deletes an assignment by its ID.
    /// </summary>
    /// <param name="id">The ID of the assignment to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if no assignment with the given ID exists.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        if (Assignments.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Assignment with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(Assignments, Config.s_assignments_xml);
    }

    /// <summary>
    /// Deletes all assignments from the system.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Assignment>(), Config.s_assignments_xml);
    }

    /// <summary>
    /// Updates an existing assignment.
    /// </summary>
    /// <param name="item">The assignment object with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if no assignment with the given ID exists.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Assignment item)
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        if (Assignments.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Assignment with ID={item.Id} does Not exist");
        Assignments.Add(item);
        XMLTools.SaveListToXMLSerializer(Assignments, Config.s_assignments_xml);
    }

    /// <summary>
    /// Creates a new assignment.
    /// </summary>
    /// <param name="item">The assignment object to create.</param>
    /// <exception cref="DalAlreadyExistsException">Thrown if an assignment with the same ID already exists.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Assignment item)
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);

        if (Assignments.Any(a => a.Id == item.Id))
            throw new DalAlreadyExistsException($"Assignment with ID={item.Id} already exists");

        int newId = Config.NextAssignmentId;
        Assignment newAssignment = item with { Id = newId };

        Assignments.Add(newAssignment);
        XMLTools.SaveListToXMLSerializer(Assignments, Config.s_assignments_xml);
    }

    /// <summary>
    /// Reads an assignment by its ID.
    /// </summary>
    /// <param name="id">The ID of the assignment to retrieve.</param>
    /// <returns>The assignment object if found; otherwise, null.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(int id)
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return Assignments.FirstOrDefault(a => a.Id == id);
    }

    /// <summary>
    /// Reads the first assignment that matches the given filter.
    /// </summary>
    /// <param name="filter">A predicate to filter the assignments.</param>
    /// <returns>The first assignment that matches the filter; otherwise, null.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(Func<Assignment, bool> filter)
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return Assignments.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all assignments.
    /// </summary>
    /// <returns>A list of all assignment objects.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public List<Assignment> ReadAll()
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return Assignments;
    }

    /// <summary>
    /// Reads all assignments that match the given filter.
    /// </summary>
    /// <param name="filter">An optional predicate to filter the assignments.</param>
    /// <returns>An enumerable of assignments that match the filter; or all assignments if no filter is provided.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null) // Stage 2
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);

        return filter == null
            ? Assignments.Select(item => item)
            : Assignments.Where(filter);
    }
}
