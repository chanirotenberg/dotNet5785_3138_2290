namespace BlImplementation;
using BlApi;
using Helpers;
using System.Collections.Generic;

internal class CallImplementation : ICall
{

    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    /// <summary>
    /// Gets the quantity of calls by their derived status (e.g., InTreatment, InRiskTreatment).
    /// </summary>
    /// <returns>An array with the count of calls for each derived status.</returns>
    public int[] GetCallCountsByStatus()
    {
        try
        {
            return _dal.Call.ReadAll()
                .GroupBy(c => CallManager.DetermineCallStatus(c.Id))
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToArray();
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to retrieve call quantities by derived status.", ex);
        }
    }

    /// <summary>
    /// Retrieves a filtered and sorted list of calls.
    /// </summary>
    /// <param name="filterField">The field to filter by.</param>
    /// <param name="filterValue">The value to filter with.</param>
    /// <param name="sortField">The field to sort by.</param>
    /// <returns>A filtered and sorted list of calls.</returns>
    public IEnumerable<BO.CallInList> GetCallList(BO.CallSortField? filterField = null, object? filterValue = null, BO.CallSortField? sortField = null)
    {
        try
        {
            var calls = _dal.Call.ReadAll()
                .Select(c =>
                {
                    var lastAssignment = _dal.Assignment.ReadAll(a => a.CallId == c.Id)
                        .OrderByDescending(a => a.EntryTime)
                        .FirstOrDefault();

                    return new BO.CallInList
                    {
                        Id = lastAssignment?.Id,
                        CallId = c.Id,
                        CallType = (BO.CallType)c.CallType,
                        OpeningTime = c.OpeningTime,
                        RemainingTime = c.MaximumTime.HasValue ? c.MaximumTime.Value - _dal.Config.Clock : null,
                        LastVolunteerName = lastAssignment != null ? _dal.Volunteer.Read(lastAssignment.VolunteerId)?.Name : null,
                        TotalTreatmentTime = lastAssignment?.ActualEndTime.HasValue == true ? lastAssignment.ActualEndTime.Value - lastAssignment.EntryTime : null,
                        Status = (BO.CallStatus)CallManager.DetermineCallStatus(c.Id),
                        SumOfAssignments = _dal.Assignment.ReadAll(a => a.CallId == c.Id).Count()
                    };
                });

            if (filterField.HasValue && filterValue != null)
            {
                calls = calls.Where(c => c.GetType().GetProperty(filterField.ToString())?.GetValue(c)?.Equals(filterValue) == true);
            }

            return sortField.HasValue
                ? calls.OrderBy(c => c.GetType().GetProperty(sortField.ToString())?.GetValue(c))
                : calls.OrderBy(c => c.CallId);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to retrieve calls list.", ex);
        }
    }


    /// <summary>
    /// Retrieves the details of a specific call by ID.
    /// </summary>
    /// <param name="callId">The ID of the call to retrieve.</param>
    /// <returns>The detailed call information.</returns>
    public BO.Call GetCallDetails(int callId)
    {
        try
        {
            var doCall = _dal.Call.Read(callId) ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist.");
            var assignments = _dal.Assignment.ReadAll(a => a.CallId == callId)
                .Select(a => new BO.CallAssignInList
                {
                    VolunteerId = a.VolunteerId,
                    Name = _dal.Volunteer.Read(a.VolunteerId)?.Name,
                    EntryTime = a.EntryTime,
                    EndTime = a.ActualEndTime,
                    EndType = (BO.EndType?)a.EndType
                }).ToList();

            return new BO.Call
            {
                Id = doCall.Id,
                CallType = (BO.CallType)doCall.CallType,
                VerbalDescription = doCall.VerbalDescription,
                Address = doCall.Address,
                Latitude = doCall.Latitude,
                Longitude = doCall.Longitude,
                OpeningTime = doCall.OpeningTime,
                MaximumTime = doCall.MaximumTime,
                Assignments = assignments,
                Status = (BO.CallStatus)CallManager.DetermineCallStatus(doCall.Id) // Add derived status
            };
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"Failed to retrieve details for call with ID={callId}.", ex);
        }
    }


    /// <summary>
    /// Updates a call with new details.
    /// </summary>
    /// <param name="call">The call to update.</param>
    public void UpdateCall(BO.Call call)
    {
        try
        {
            CallManager.ValidateCall(call);

            var doCall = _dal.Call.Read(call.Id) ?? throw new BO.BlDoesNotExistException($"Call with ID={call.Id} does not exist.");

            var updatedCall = doCall with
            {
                CallType = (DO.CallType)call.CallType,
                VerbalDescription = call.VerbalDescription,
                Address = call.Address,
                Latitude = call.Latitude,
                Longitude = call.Longitude,
                OpeningTime = call.OpeningTime,
                MaximumTime = call.MaximumTime
            };

            _dal.Call.Update(updatedCall);
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"Failed to update call with ID={call.Id}.", ex);
        }
    }

    /// <summary>
    /// Deletes a specific call by ID.
    /// </summary>
    /// <param name="callId">The ID of the call to delete.</param>
    public void DeleteCall(int callId)
    {
        try
        {
            var call = _dal.Call.Read(callId) ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist.");
            var hasAssignments = _dal.Assignment.ReadAll(a => a.CallId == callId).Any();

            if (CallManager.DetermineCallStatus(call.Id) != 0 || hasAssignments) // Only delete if not in treatment or in risk
                throw new BO.BlDeletionImpossibleException($"Cannot delete call with ID={callId} as it is either in treatment or in risk.");

            _dal.Call.Delete(callId);
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"Failed to delete call with ID={callId}.", ex);
        }
    }

    /// <summary>
    /// Adds a new call to the system.
    /// </summary>
    /// <param name="call">The BO.Call object to add.</param>
    public void AddCall(BO.Call call)
    {
        try
        {
            CallManager.ValidateCall(call);

            var doCall = new DO.Call
            {
                Id =call.Id,
                CallType = (DO.CallType)call.CallType,
                VerbalDescription = call.VerbalDescription,
                Address = call.Address,
                Latitude = call.Latitude,
                Longitude = call.Longitude,
                OpeningTime = call.OpeningTime,
                MaximumTime = call.MaximumTime
            };

            _dal.Call.Create(doCall);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to add a new call.", ex);
        }
    }

    /// <summary>
    /// Retrieves closed calls handled by a volunteer.
    /// </summary>
    /// <param name="volunteerId">The volunteer ID.</param>
    /// <param name="callType">The type of call to filter by.</param>
    /// <param name="sortBy">The field to sort by.</param>
    /// <returns>A list of closed calls.</returns>
    public IEnumerable<BO.ClosedCallInList> GetClosedCallsByVolunteer(int volunteerId, BO.CallType? callType = null, BO.CallSortField? sortBy = null)
    {
        try
        {
            var assignments = _dal.Assignment.ReadAll(a => a.VolunteerId == volunteerId && a.EndType != null)
                .Where(a => callType == null || (BO.CallType)_dal.Call.Read(a.CallId).CallType == callType)
                .Select(a =>
                {
                    var call = _dal.Call.Read(a.CallId);
                    return new BO.ClosedCallInList
                    {
                        Id = call.Id,
                        CallType = (BO.CallType)call.CallType,
                        Address = call.Address,
                        OpeningTime = call.OpeningTime,
                        EntryTime = a.EntryTime,
                        EndTime = a.ActualEndTime,
                        EndType = (BO.EndType)a.EndType
                    };
                });

            return sortBy.HasValue
                ? assignments.OrderBy(a => a.GetType().GetProperty(sortBy.ToString())?.GetValue(a))
                : assignments.OrderBy(a => a.Id);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to retrieve closed calls by volunteer.", ex);
        }
    }

    /// <summary>
    /// Retrieves open calls available for a volunteer to choose from.
    /// </summary>
    /// <param name="volunteerId">The volunteer ID.</param>
    /// <param name="callType">The type of call to filter by.</param>
    /// <param name="sortBy">The field to sort by.</param>
    /// <returns>A list of open calls with distance information.</returns>
    public IEnumerable<BO.OpenCallInList> GetOpenCallsForVolunteer(int volunteerId, BO.CallType? callType = null, BO.CallSortField? sortBy = null)
    {
        try
        {
            var volunteer = _dal.Volunteer.Read(volunteerId)
                            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={volunteerId} does not exist.");

            var calls = _dal.Call.ReadAll()
                .Where(c => CallManager.DetermineCallStatus(c.Id) == 0 || CallManager.DetermineCallStatus(c.Id) == 5)
                .Where(c => callType == null || (BO.CallType)c.CallType == callType)
                .Select(c => new BO.OpenCallInList
                {
                    Id = c.Id,
                    CallType = (BO.CallType)c.CallType,
                    VerbalDescription = c.VerbalDescription,
                    Address = c.Address,
                    OpeningTime = c.OpeningTime,
                    MaximumTime = c.MaximumTime,
                    DistanceFromVolunteer = CallManager.CalculateAirDistance(volunteer.Address, c.Address)
                });

            return sortBy.HasValue
                ? calls.OrderBy(c => c.GetType().GetProperty(sortBy.ToString())?.GetValue(c))
                : calls.OrderBy(c => c.Id);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to retrieve open calls for volunteer.", ex);
        }
    }
    /// <summary>
    /// Closes a call by marking the assignment as completed.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer closing the call.</param>
    /// <param name="assignmentId">The ID of the assignment to be closed.</param>
    
    public void CloseCall(int volunteerId, int assignmentId)
    {
        try
        {
            var assignment = _dal.Assignment.Read(assignmentId)
                ?? throw new BO.BlDoesNotExistException($"Assignment with ID={assignmentId} does not exist.");

            if (assignment.VolunteerId != volunteerId)
                throw new BO.BlAuthorizationException($"Volunteer with ID={volunteerId} is not authorized to close this call.");

            if (assignment.ActualEndTime != null || assignment.EndType != null)
                throw new BO.BlLogicException($"The assignment with ID={assignmentId} is already closed or expired.");

            assignment = assignment with
            {
                ActualEndTime = _dal.Config.Clock,
                EndType = DO.EndType.Cared
            };

            _dal.Assignment.Update(assignment);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to close the call.", ex);
        }
    }

    /// <summary>
    /// Cancels an assignment for a call.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the cancellation.</param>
    /// <param name="assignmentId">The ID of the assignment to be canceled.</param>
   
    public void CancelCall(int requesterId, int assignmentId)
    {
        try
        {
            var assignment = _dal.Assignment.Read(assignmentId)
                ?? throw new BO.BlDoesNotExistException($"Assignment with ID={assignmentId} does not exist.");

            if (assignment.ActualEndTime != null || assignment.EndType != null)
                throw new BO.BlLogicException($"The assignment with ID={assignmentId} is already closed or expired.");

            var isRequesterAuthorized =
                assignment.VolunteerId == requesterId ||
                 _dal.Volunteer.Read(requesterId)?.Jobs == DO.Jobs.Administrator;

            if (!isRequesterAuthorized)
                throw new BO.BlAuthorizationException($"Requester with ID={requesterId} is not authorized to cancel this call.");

            assignment = assignment with
            {
                ActualEndTime = _dal.Config.Clock,
                EndType = assignment.VolunteerId == requesterId ? DO.EndType.SelfCancellation : DO.EndType.AdministratorCancellation
            };

            _dal.Assignment.Update(assignment);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to cancel the call.", ex);
        }
    }

    /// <summary>
    /// Assigns a call to a volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <param name="callId">The ID of the call to be assigned.</param>
    
    public void AssignCallToVolunteer(int volunteerId, int callId)
    {
        try
        {
            var call = _dal.Call.Read(callId)
                ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist.");

            var isCallOpen = CallManager.DetermineCallStatus(call.Id) == 0 || CallManager.DetermineCallStatus(call.Id) == 5;
            if (!isCallOpen)
                throw new BO.BlLogicException($"Call with ID={callId} is not open for assignment.");

            var existingAssignments = _dal.Assignment.ReadAll(a => a.CallId == callId && a.ActualEndTime == null);
            if (existingAssignments.Any())
                throw new BO.BlLogicException($"Call with ID={callId} is already assigned to a volunteer.");

            var assignment = new DO.Assignment
            {
                CallId = callId,
                VolunteerId = volunteerId,
                EntryTime = _dal.Config.Clock,
                ActualEndTime = null,
                EndType = null
            };

            _dal.Assignment.Create(assignment);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to assign call to volunteer.", ex);
        }
    }

}
