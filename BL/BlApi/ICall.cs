
using BO;

namespace BlApi;

/// <summary>
/// Interface for managing calls in the system.
/// </summary>
public interface ICall : IObservable //stage 5
{
    /// <summary>
    /// Retrieves an array of call counts by their status.
    /// </summary>
    /// <returns>Array of integers representing the count of calls in each status.</returns>
    int[] GetCallCountsByStatus();

    /// <summary>
    /// Retrieves a list of calls, filtered and sorted by specified parameters.
    /// </summary>
    /// <param name="filterField">Field to filter the list by (nullable).</param>
    /// <param name="filterValue">Value to filter by (nullable).</param>
    /// <param name="sortBy">Field to sort the list by (nullable).</param>
    /// <returns>A collection of calls represented by CallInList.</returns>
    public IEnumerable<BO.CallInList> GetCallList(BO.CallSortAndFilterField? filterField = null, object? filterValue = null, BO.CallSortAndFilterField? sortField = null);

    /// <summary>
    /// Retrieves detailed information about a specific call by its ID.
    /// </summary>
    /// <param name="id">The ID of the call to retrieve.</param>
    /// <returns>A Call object representing the call details.</returns>
    BO.Call GetCallDetails(int id);

    /// <summary>
    /// Updates the details of an existing call.
    /// </summary>
    /// <param name="call">The Call object containing updated call information.</param>
    void UpdateCall(BO.Call call);

    /// <summary>
    /// Deletes a call by its ID.
    /// </summary>
    /// <param name="id">The ID of the call to delete.</param>
    void DeleteCall(int id);

    /// <summary>
    /// Adds a new call to the system.
    /// </summary>
    /// <param name="call">The Call object representing the new call to add.</param>
    void AddCall(BO.Call call);

    /// <summary>
    /// Retrieves a list of closed calls handled by a specific volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <param name="callType">Optional filter by call type.</param>
    /// <param name="sortBy">Optional sorting field.</param>
    /// <returns>A collection of closed calls represented by ClosedCallInList.</returns>
    IEnumerable<BO.ClosedCallInList> GetClosedCallsByVolunteer(int volunteerId, BO.CallType? callType = null, BO.CallSortAndFilterField? sortBy = null);

    /// <summary>
    /// Retrieves a list of open calls available for a specific volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <param name="callType">Optional filter by call type.</param>
    /// <param name="sortBy">Optional sorting field.</param>
    /// <returns>A collection of open calls represented by OpenCallInList.</returns>
    IEnumerable<BO.OpenCallInList> GetOpenCallsForVolunteer(int volunteerId, BO.CallType? callType = null, BO.CallSortAndFilterField? sortBy = null);

    /// <summary>
    /// Marks a call as completed by a volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer completing the call.</param>
    /// <param name="assignmentId">The ID of the assignment associated with the call.</param>
    void CloseCall(int volunteerId, int assignmentId);

    /// <summary>
    /// Cancels a call assignment by a volunteer or manager.
    /// </summary>
    /// <param name="requesterId">The ID of the person requesting the cancellation.</param>
    /// <param name="assignmentId">The ID of the assignment to cancel.</param>
    void CancelCall(int requesterId, int assignmentId);

    /// <summary>
    /// Assigns a call to a volunteer for handling.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer to assign the call to.</param>
    /// <param name="callId">The ID of the call to assign.</param>
    void AssignCallToVolunteer(int volunteerId, int callId);
}
