namespace Dal;
using DalApi;
using DO;

internal class CallImplementation : ICall
{
    /// <summary>
    /// Creates a new call.
    /// </summary>
    /// <param name="item">The call to be created.</param>
    /// <exception cref="Exception">Thrown if a call with the given ID already exists.</exception>
    public void Create(Call item)
    {
        if (this.Read(item.Id) is not null)
            throw new Exception("An call with this ID already exists.");
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
    /// Reads all calls.
    /// </summary>
    /// <returns>A list of all call objects.</returns>
    public List<Call> ReadAll()
    {
        return new List<Call>(DataSource.Calls);
    }

    /// <summary>
    /// Deletes a call by its ID.
    /// </summary>
    /// <param name="id">The ID of the call to be deleted.</param>
    /// <exception cref="Exception">Thrown if the call with the given ID does not exist.</exception>
    public void Delete(int id)
    {
        Call? call = DataSource.Calls.FirstOrDefault(c => c.Id == id) ?? throw new Exception($"Call Object with {id} doesn't exist");
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
    /// <exception cref="Exception">Thrown if the call with the given ID does not exist.</exception>
    public void Update(Call item)
    {
        Call? existingCall = DataSource.Calls.FirstOrDefault(c => c.Id == item.Id) ?? throw new Exception($"Call Object with {item.Id} doesn't exist");

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
}
