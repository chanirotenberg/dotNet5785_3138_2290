namespace BlImplementation;
using BlApi;
using BO;
using DO;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

internal class CallImplementation : ICall
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public int[] GetCallCountsByStatus()
    {
        try
        {
            lock (AdminManager.BlMutex)
            {
                return _dal.Call.ReadAll()
                    .GroupBy(c => CallManager.DetermineCallStatus(c.Id))
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .ToArray();
            }
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to retrieve call quantities by derived status.", ex);
        }
    }

    public IEnumerable<BO.CallInList> GetCallList(BO.CallSortAndFilterField? filterField = null, object? filterValue = null, BO.CallSortAndFilterField? sortField = null)
    {
        try
        {
            IEnumerable<BO.CallInList> calls;

            lock (AdminManager.BlMutex)
            {
                calls = _dal.Call.ReadAll()
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
                       
                    }).ToList();
            }
            CallManager.Observers.NotifyListUpdated();

            if (filterField.HasValue && filterValue != null)
            {
                calls = filterField switch
                {
                    BO.CallSortAndFilterField.Id => calls.Where(c => c.CallId == (filterValue as int? ?? -1)),
                    BO.CallSortAndFilterField.Status => calls.Where(c => c.Status == (filterValue as BO.CallStatus? ?? BO.CallStatus.Open)),
                    BO.CallSortAndFilterField.CallType => calls.Where(c => c.CallType == (filterValue as BO.CallType? ?? BO.CallType.None)),
                    BO.CallSortAndFilterField.OpeningTime => calls.Where(c => c.OpeningTime == (filterValue as DateTime? ?? DateTime.MinValue)),
                    _ => calls
                };
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

    public BO.Call GetCallDetails(int callId)
    {
        try
        {
            DO.Call doCall;
            List<BO.CallAssignInList> assignments;

            lock (AdminManager.BlMutex)
            {
                doCall = _dal.Call.Read(callId) ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist.");

                assignments = _dal.Assignment.ReadAll(a => a.CallId == callId)
                    .Select(a => new BO.CallAssignInList
                    {
                        VolunteerId = a.VolunteerId,
                        Name = _dal.Volunteer.Read(a.VolunteerId)?.Name,
                        EntryTime = a.EntryTime,
                        EndTime = a.ActualEndTime,
                        EndType = (BO.EndType?)a.EndType
                    }).ToList();
            }

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
                Status = (BO.CallStatus)CallManager.DetermineCallStatus(doCall.Id)
            };
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"Failed to retrieve details for call with ID={callId}.", ex);
        }
    }

    public void UpdateCall(BO.Call call)
    {
        try
        {
            AdminManager.ThrowOnSimulatorIsRunning();

            CallManager.ValidateCall(call);

            DO.Call doCall;
            DO.Call updatedCall;
            lock (AdminManager.BlMutex)
            {
                doCall = _dal.Call.Read(call.Id) ?? throw new BO.BlDoesNotExistException($"Call with ID={call.Id} does not exist.");

                updatedCall = doCall with
                {
                    CallType = (DO.CallType)call.CallType,
                    VerbalDescription = call.VerbalDescription,
                    Address = call.Address,
                    OpeningTime = call.OpeningTime,
                    MaximumTime = call.MaximumTime
                };

                _dal.Call.Update(updatedCall);
            }

            CallManager.Observers.NotifyItemUpdated(doCall.Id);
            CallManager.Observers.NotifyListUpdated();

            _ = CallManager.UpdateCoordinatesForCallAsync(updatedCall);
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"Failed to update call with ID={call.Id}.", ex);
        }
    }

    public void DeleteCall(int callId)
    {
        try
        {
            AdminManager.ThrowOnSimulatorIsRunning();

            bool shouldDelete = false;

            lock (AdminManager.BlMutex)
            {
                var call = _dal.Call.Read(callId) ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist.");
                var hasAssignments = _dal.Assignment.ReadAll(a => a.CallId == callId).Any();

                if (CallManager.DetermineCallStatus(call.Id) == 0 && !hasAssignments)
                    shouldDelete = true;

                if (shouldDelete)
                    _dal.Call.Delete(callId);
                else
                    throw new BO.BlDeletionImpossibleException($"Cannot delete call with ID={callId} as it is either in treatment or in risk or has assignments.");
            }

            CallManager.Observers.NotifyListUpdated();
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"Failed to delete call with ID={callId}.", ex);
        }
    }

    public void AddCall(BO.Call call)
    {
        try
        {
            AdminManager.ThrowOnSimulatorIsRunning();

            CallManager.ValidateCall(call);

            var doCall = new DO.Call
            {
                Id = call.Id,
                CallType = (DO.CallType)call.CallType,
                VerbalDescription = call.VerbalDescription,
                Address = call.Address,
                OpeningTime = call.OpeningTime,
                MaximumTime = call.MaximumTime
            };

            lock (AdminManager.BlMutex)
            {
                _dal.Call.Create(doCall);
            }
            
            CallManager.Observers.NotifyListUpdated();

            _=CallManager.SendNewCallEmailAsync(call);

            _ = CallManager.UpdateCoordinatesForCallAsync(doCall);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to add a new call.", ex);
        }
    }

    public IEnumerable<BO.ClosedCallInList> GetClosedCallsByVolunteer(int volunteerId, BO.CallType? callType = null, BO.CallSortAndFilterField? sortBy = null)
    {
        try
        {
            List<BO.ClosedCallInList> assignments;

            lock (AdminManager.BlMutex)
            {
                assignments = _dal.Assignment.ReadAll(a => a.VolunteerId == volunteerId && a.EndType != null)
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
                    }).ToList();
            }
            CallManager.Observers.NotifyListUpdated();

            return sortBy.HasValue
                ? assignments.OrderBy(a => a.GetType().GetProperty(sortBy.ToString())?.GetValue(a))
                : assignments.OrderBy(a => a.Id);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("Failed to retrieve closed calls by volunteer.", ex);
        }
    }

    public IEnumerable<BO.OpenCallInList> GetOpenCallsForVolunteer(int volunteerId, BO.CallType? callType = null, BO.CallSortAndFilterField? sortBy = null)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        return VolunteerManager.GetOpenCallsForVolunteer(volunteerId, callType, sortBy);
    }
       

    public void CloseCall(int volunteerId, int assignmentId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        VolunteerManager.CloseCall(volunteerId, assignmentId);
    }

    public void CancelCall(int requesterId, int assignmentId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        VolunteerManager.CancelCall(requesterId, assignmentId);
    }

    public void AssignCallToVolunteer(int volunteerId, int callId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        VolunteerManager.AssignCallToVolunteer(volunteerId, callId);
    }
    public bool CallHasCoordinates(int callId)
    {
        var call = _dal.Call.Read(callId);
        return call.Latitude != null && call.Longitude != null;
    }

    #region Stage 5
    public void AddObserver(Action listObserver) =>
        CallManager.Observers.AddListObserver(listObserver); //stage 5

    public void AddObserver(int id, Action observer) =>
        CallManager.Observers.AddObserver(id, observer); //stage 5

    public void RemoveObserver(Action listObserver) =>
        CallManager.Observers.RemoveListObserver(listObserver); //stage 5

    public void RemoveObserver(int id, Action observer) =>
        CallManager.Observers.RemoveObserver(id, observer); //stage 5
    #endregion Stage 5
}
