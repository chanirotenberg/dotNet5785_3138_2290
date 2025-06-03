namespace Dal;
using DalApi;
using DO;
using System.Linq;

internal class CallImplementation : ICall
{
    /// <summary>
    /// Creates a new call.
    /// </summary>
    /// <param name="item">The call to be created.</param>
    /// <exception cref="DalAlreadyExistsException">Thrown if a call with the given ID already exists.</exception>
    public void Create(Call item)
    {
        int newId = Config.NextCallId;
        Call newCall = item with { Id = newId};
        DataSource.Calls.Add(newCall);
    }

    /// <summary>
    /// Reads (retrieves) a call by its ID.
    /// </summary>
    /// <param name="id">The ID of the call to be retrieved.</param>
    /// <returns>The call object if found; otherwise, null.</returns>
    public Call? Read(int id)
    {
        return DataSource.Calls.FirstOrDefault(a => a.Id == id);
    }

    /// <summary>
    /// Reads all calls with an optional filter.
    /// </summary>
    /// <param name="filter">An optional filter function to apply to the calls.</param>
    /// <returns>A list of all call objects, filtered if the filter is provided.</returns>
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null) //stage 2
        => filter == null
            ? DataSource.Calls.Select(item => item)
            : DataSource.Calls.Where(filter);

    /// <summary>
    /// Deletes a call by its ID.
    /// </summary>
    /// <param name="id">The ID of the call to be deleted.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the call with the given ID does not exist.</exception>
    public void Delete(int id)
    {
        Call? call = DataSource.Calls.FirstOrDefault(c => c.Id == id) ?? throw new DalDoesNotExistException($"Call Object with {id} doesn't exist");
        DataSource.Calls.Remove(call);
    }

    /// <summary>
    /// Deletes all calls.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Calls.Clear();
    }

    /// <summary>
    /// Updates an existing call with new values.
    /// </summary>
    /// <param name="item">The call object with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the call with the given ID does not exist.</exception>
    public void Update(Call item)
    {
        Call? existingCall = DataSource.Calls.FirstOrDefault(c => c.Id == item.Id) ?? throw new DalDoesNotExistException($"Call Object with {item.Id} doesn't exist");

        // Create a new call object with updated values
        Call updatedCall = existingCall with
        {
            CallType = item.CallType,
            VerbalDescription = item.VerbalDescription,
            Address = item.Address,
            Latitude = item.Latitude,
            Longitude = item.Longitude,
            OpeningTime = item.OpeningTime,
            MaximumTime = item.MaximumTime
        };

        // Remove the old call and add the updated one
        DataSource.Calls.Remove(existingCall);
        DataSource.Calls.Add(updatedCall);
    }

    /// <summary>
    /// Reads a call based on a custom filter.
    /// </summary>
    /// <param name="filter">The filter function to be applied on the call.</param>
    /// <returns>The first call that matches the filter, or null if none matches.</returns>
    public Call? Read(Func<Call, bool> filter)
    {
        return DataSource.Calls.FirstOrDefault(filter);
    }
}
