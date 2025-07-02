namespace BlImplementation;
using BlApi;
using BO;
using DO;
using Helpers;
using System;
using System.Xml.Linq;

internal class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public BO.Jobs Login(int username, string password)
    {
        try
        {
            DO.Volunteer volunteer;
            lock (AdminManager.BlMutex)
            {
                volunteer = _dal.Volunteer.Read(v => v.Id == username)
                            ?? throw new BO.BlDoesNotExistException($"Volunteer with username '{username}' does not exist.");
            }
            var encryptPassword = VolunteerManager.EncryptPassword(password);

            if (volunteer.Password != encryptPassword)
                throw new BO.BlInvalidValueException($"Invalid password for volunteer with username '{username}'.");

            return (BO.Jobs)volunteer.Jobs;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer with username '{username}' not found.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"An error occurred during login for username '{username}'.", ex);
        }
    }

    public IEnumerable<BO.VolunteerInList> GetVolunteerList(bool? isActive = null, BO.VolunteerSortField? sortBy = null)
    {
        try
        {
            List<BO.VolunteerInList> volunteers;
            lock (AdminManager.BlMutex)
            {
                volunteers = _dal.Volunteer.ReadAll()
                    .Select(v =>
                    {
                        var currentCallId = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && a.EndType == null)
                            .Select(a => (int?)a.CallId).FirstOrDefault();

                        BO.CallType callType = currentCallId.HasValue
                            ? (BO.CallType)(_dal.Call.Read(currentCallId.Value)?.CallType ?? DO.CallType.None)
                            : BO.CallType.None;

                        return new BO.VolunteerInList
                        {
                            Id = v.Id,
                            Name = v.Name,
                            IsActive = v.active,
                            TotalHandledCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && a.EndType == DO.EndType.Cared).Count(),
                            TotalCanceledCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && (a.EndType == DO.EndType.AdministratorCancellation || a.EndType == DO.EndType.SelfCancellation)).Count(),
                            TotalExpiredCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && a.EndType == DO.EndType.ExpiredCancellation).Count(),
                            CurrentCallId = currentCallId,
                            CallType = callType
                        };
                    }).ToList();
            }

            if (isActive.HasValue)
                volunteers = volunteers.Where(v => v.IsActive == isActive).ToList();

            return sortBy switch
            {
                BO.VolunteerSortField.Name => volunteers.OrderBy(v => v.Name),
                BO.VolunteerSortField.SumOfCalls => volunteers.OrderByDescending(v => v.TotalHandledCalls),
                BO.VolunteerSortField.SumOfCancellation => volunteers.OrderByDescending(v => v.TotalCanceledCalls),
                BO.VolunteerSortField.SumOfExpiredCalls => volunteers.OrderByDescending(v => v.TotalExpiredCalls),
                _ => volunteers.OrderBy(v => v.Id),
            };
        }
        catch (Exception ex)
        {
            throw new BO.BlException("An error occurred while retrieving the volunteer list.", ex);
        }
    }

    public void CreateVolunteer(BO.Volunteer boVolunteer)
    {
        try
        {
            AdminManager.ThrowOnSimulatorIsRunning();

            VolunteerManager.ValidateVolunteer(boVolunteer, isPartial: true);

            var doVolunteer = new DO.Volunteer
            {
                Id = boVolunteer.Id,
                Name = boVolunteer.Name,
                Phone = boVolunteer.Phone,
                Email = boVolunteer.Email,
                Password = boVolunteer.Password,
                Address = boVolunteer.Address,
                Jobs = (DO.Jobs)boVolunteer.Jobs,
                active = boVolunteer.IsActive,
                MaxDistance = boVolunteer.MaxDistance,
                DistanceType = (DO.DistanceType)boVolunteer.DistanceType
            };

            lock (AdminManager.BlMutex)
            {
                _dal.Volunteer.Create(doVolunteer);
            }

            VolunteerManager.Observers.NotifyListUpdated(); // stage 5

            _=VolunteerManager.CompleteCoordinatesAsync(doVolunteer);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlDuplicateEntityException($"Volunteer with ID={boVolunteer.Id} already exists.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"An error occurred while creating volunteer with ID={boVolunteer.Id}.", ex);
        }
    }
  
   
    public BO.Volunteer GetVolunteerDetails(int id)
    {
        try
        {
            DO.Volunteer doVolunteer;
            DO.Assignment? activeAssignment;

            lock (AdminManager.BlMutex)
            {
                doVolunteer = _dal.Volunteer.Read(id)
                    ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist.");

                activeAssignment = _dal.Assignment.ReadAll(a => a.VolunteerId == doVolunteer.Id && a.EndType == null)
                    .FirstOrDefault();
            }

            BO.CallInProgress? callInProgress = null;

            if (activeAssignment is not null)
            {
                DO.Call? call;
                lock (AdminManager.BlMutex)
                {
                    call = _dal.Call.Read(activeAssignment.CallId);
                }
                if (call is not null)
                {
                    callInProgress = new BO.CallInProgress
                    {
                        Id = activeAssignment.Id,
                        CallId = call.Id,
                        CallType = (BO.CallType)call.CallType,
                        VerbalDescription = call.VerbalDescription,
                        Address = call.Address,
                        OpeningTime = call.OpeningTime,
                        MaximumTime = call.MaximumTime,
                        EntryTime = activeAssignment.EntryTime,
                        Distance = CallManager.CalculateDistance(doVolunteer, call.Address),
                        Status = (BO.CallStatus)CallManager.DetermineCallStatus(call.Id)
                    };
                }
            }

            int sumOfCalls, sumOfCancellation, sumOfExpiredCalls;
            lock (AdminManager.BlMutex)
            {
                sumOfCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == doVolunteer.Id && a.EndType == DO.EndType.Cared).Count();
                sumOfCancellation = _dal.Assignment.ReadAll(a => a.VolunteerId == doVolunteer.Id && a.EndType == DO.EndType.AdministratorCancellation).Count();
                sumOfExpiredCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == doVolunteer.Id && a.EndType == DO.EndType.ExpiredCancellation).Count();
            }

            return new BO.Volunteer
            {
                Id = doVolunteer.Id,
                Name = doVolunteer.Name,
                Phone = doVolunteer.Phone,
                Email = doVolunteer.Email,
                Password = doVolunteer.Password,
                Address = doVolunteer.Address,
                Latitude = doVolunteer.Latitude,
                Longitude = doVolunteer.Longitude,
                Jobs = (BO.Jobs)doVolunteer.Jobs,
                IsActive = doVolunteer.active,
                MaxDistance = doVolunteer.MaxDistance,
                DistanceType = (BO.DistanceType)doVolunteer.DistanceType,
                SumOfCalls = sumOfCalls,
                SumOfCancellation = sumOfCancellation,
                SumOfExpiredCalls = sumOfExpiredCalls,
                CallInProgress = callInProgress
            };
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"An error occurred while retrieving details for volunteer with ID={id}.", ex);
        }
    }

    public void DeleteVolunteer(int id)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        try
        {
            DO.Volunteer volunteer;
            bool hasCalls;
            int adminCount;

            lock (AdminManager.BlMutex)
            {
                volunteer = _dal.Volunteer.Read(id)
                    ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist.");

                hasCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == id).Any();

                if (volunteer.Jobs == DO.Jobs.Administrator)
                {
                    adminCount = _dal.Volunteer.ReadAll(v => v.Jobs == DO.Jobs.Administrator && v.Id != id).Count();
                    if (adminCount == 0)
                        throw new BO.BlException("Cannot delete the only administrator in the system.");
                }
                else
                {
                    adminCount = 0;
                }

                if (hasCalls)
                    throw new BO.BlDeletionImpossibleException($"Cannot delete volunteer with ID={id} as they have associated assignments.");

                _dal.Volunteer.Delete(id);
            }

            VolunteerManager.Observers.NotifyListUpdated(); // stage 5
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"An error occurred while deleting volunteer with ID={id}.", ex);
        }
    }

    public void UpdateVolunteer(int requesterId, BO.Volunteer boVolunteer)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        try
        {
            DO.Volunteer requester;
            DO.Volunteer currentVolunteer;
            int adminCount;

            lock (AdminManager.BlMutex)
            {
                requester = _dal.Volunteer.Read(requesterId)
                    ?? throw new BO.BlDoesNotExistException($"Requester with ID={requesterId} does not exist.");

                if (requesterId != boVolunteer.Id && requester.Jobs != DO.Jobs.Administrator)
                    throw new BO.BlAuthorizationException($"Requester with ID={requesterId} is not authorized to update volunteer with ID={boVolunteer.Id}.");

                if (boVolunteer.IsActive == false)
                {
                    var activeAssignment = _dal.Assignment.ReadAll(a => a.VolunteerId == boVolunteer.Id && a.ActualEndTime == null)
                        .FirstOrDefault();

                    if (activeAssignment != null)
                        throw new BO.BlException($"Volunteer with ID={boVolunteer.Id} is currently assigned to an open call and cannot be deactivated.");
                }

                currentVolunteer = _dal.Volunteer.Read(boVolunteer.Id)
                    ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={boVolunteer.Id} does not exist.");

                if (currentVolunteer.Jobs == DO.Jobs.Administrator && (DO.Jobs)boVolunteer.Jobs == DO.Jobs.Worker)
                {
                    adminCount = _dal.Volunteer.ReadAll(v => v.Jobs == DO.Jobs.Administrator && v.Id != boVolunteer.Id).Count();
                    if (adminCount == 0)
                        throw new BO.BlException("Cannot remove administrator role because this is the only administrator.");
                }
                else
                {
                    adminCount = 0;
                }
            }

            VolunteerManager.ValidateVolunteer(boVolunteer, isPartial: true, requester.Password);

            var doVolunteer = new DO.Volunteer
            {
                Id = boVolunteer.Id,
                Name = boVolunteer.Name,
                Phone = boVolunteer.Phone,
                Email = boVolunteer.Email,
                Password = boVolunteer.Password,
                Address = boVolunteer.Address,
                Jobs = (DO.Jobs)boVolunteer.Jobs,
                active = boVolunteer.IsActive,
                MaxDistance = boVolunteer.MaxDistance,
                DistanceType = (DO.DistanceType)boVolunteer.DistanceType
            };

            lock (AdminManager.BlMutex)
            {
                _dal.Volunteer.Update(doVolunteer);
            }

            VolunteerManager.Observers.NotifyItemUpdated(doVolunteer.Id);
            VolunteerManager.Observers.NotifyListUpdated();

            _=VolunteerManager.CompleteCoordinatesAsync(doVolunteer);

        }
        catch (Exception ex)
        {
            throw new BO.BlException($"An error occurred while updating volunteer with ID={boVolunteer.Id}.", ex);
        }
    }

    public IEnumerable<BO.VolunteerInList> GetVolunteersFilterList(BO.CallType? callType)
    {
        try
        {
            if (callType == null || callType == BO.CallType.None)
                return GetVolunteerList();

            return GetVolunteerList().Where(v => v.CallType == callType);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("Error accessing Volunteers.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("An unexpected error occurred.", ex);
        }
    }
    public bool VolunteerHasCoordinates(int volunteerId)
    {
        var volunteer = _dal.Volunteer.Read(volunteerId);
        return volunteer.Latitude != null && volunteer.Longitude != null;
    }

    #region Stage 5
    public void AddObserver(Action listObserver) =>
        VolunteerManager.Observers.AddListObserver(listObserver);

    public void AddObserver(int id, Action observer) =>
        VolunteerManager.Observers.AddObserver(id, observer);

    public void RemoveObserver(Action listObserver) =>
        VolunteerManager.Observers.RemoveListObserver(listObserver);

    public void RemoveObserver(int id, Action observer) =>
        VolunteerManager.Observers.RemoveObserver(id, observer);
    #endregion Stage 5
}
