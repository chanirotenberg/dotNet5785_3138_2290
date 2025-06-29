namespace Dal;

using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

internal class VolunteerImplementation : IVolunteer
{
    /// <summary>
    /// Creates a new volunteer entity in the XML file.
    /// </summary>
    /// <param name="item">The volunteer entity to be added.</param>
    /// <exception cref="DalAlreadyExistsException">
    /// Thrown if a volunteer with the same ID already exists in the XML file.
    /// </exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Volunteer item)
    {
        // Load the list of volunteers from the XML file
        List<Volunteer> Volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);

        // Check if the volunteer with the same ID already exists
        if (Volunteers.Any(a => a.Id == item.Id))
            throw new DalAlreadyExistsException($"Volunteer with ID={item.Id} already exists");

        // Add the new volunteer to the list and save it back to the XML file
        Volunteers.Add(item);
        XMLTools.SaveListToXMLSerializer(Volunteers, Config.s_volunteers_xml);
    }

    /// <summary>
    /// Reads (retrieves) a volunteer entity by its ID from the XML file.
    /// </summary>
    /// <param name="id">The ID of the volunteer to retrieve.</param>
    /// <returns>The volunteer entity if found; otherwise, null.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Volunteer? Read(int id)
    {
        List<Volunteer> Volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        return Volunteers.FirstOrDefault(a => a.Id == id);
    }

    /// <summary>
    /// Reads all volunteer entities, with an optional filter function.
    /// </summary>
    /// <param name="filter">Optional filter function to select specific volunteers.</param>
    /// <returns>An IEnumerable of volunteers based on the filter or all volunteers if no filter is provided.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null) // Stage 2
    {
        List<Volunteer> Volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);

        // Return the volunteers based on the provided filter or all of them if no filter is provided
        return filter == null
            ? Volunteers.Select(item => item)
            : Volunteers.Where(filter);
    }

    /// <summary>
    /// Deletes a volunteer entity by its ID from the XML file.
    /// </summary>
    /// <param name="id">The ID of the volunteer to delete.</param>
    /// <exception cref="DalDoesNotExistException">
    /// Thrown if the volunteer with the given ID does not exist in the XML file.
    /// </exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        List<Volunteer> Volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);

        // Attempt to remove the volunteer with the specified ID
        if (Volunteers.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Volunteer with ID={id} does not exist");

        // Save the updated list back to the XML file
        XMLTools.SaveListToXMLSerializer(Volunteers, Config.s_volunteers_xml);
    }

    /// <summary>
    /// Deletes all volunteer entities from the XML file.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        // Save an empty list of volunteers back to the XML file
        XMLTools.SaveListToXMLSerializer(new List<Volunteer>(), Config.s_volunteers_xml);
    }

    /// <summary>
    /// Updates an existing volunteer entity with new values in the XML file.
    /// </summary>
    /// <param name="item">The updated volunteer entity.</param>
    /// <exception cref="DalDoesNotExistException">
    /// Thrown if the volunteer with the given ID does not exist in the XML file.
    /// </exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Volunteer item)
    {
        List<Volunteer> Volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);

        // Attempt to remove the existing volunteer with the specified ID
        if (Volunteers.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Volunteer with ID={item.Id} does not exist");

        // Add the updated volunteer back to the list and save it to the XML file
        Volunteers.Add(item);
        XMLTools.SaveListToXMLSerializer(Volunteers, Config.s_volunteers_xml);
    }

    /// <summary>
    /// Reads a volunteer entity based on a custom filter function.
    /// </summary>
    /// <param name="filter">A filter function to find the desired volunteer.</param>
    /// <returns>The first volunteer entity that matches the filter criteria or null if no match is found.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        List<Volunteer> Volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        return Volunteers.FirstOrDefault(filter);
    }
}
