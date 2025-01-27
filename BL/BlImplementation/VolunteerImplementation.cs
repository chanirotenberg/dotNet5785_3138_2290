namespace BlImplementation;
using BlApi;
using BO;
using Helpers;

internal class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    /// <summary>
    /// Logs a volunteer into the system.
    /// </summary>
    /// <param name="username">The username of the volunteer.</param>
    /// <param name="password">The password of the volunteer.</param>
    /// <returns>The role of the volunteer as a string.</returns>
    /// <exception cref="BO.VolunteerNotFoundException">Thrown if the volunteer is not found or the password is incorrect.</exception>
    public BO.Jobs Login(string username, string password)
    {
        try
        {
            var volunteer = _dal.Volunteer.Read(v => v.Name == username)
                            ?? throw new BO.VolunteerNotFoundException("Invalid username or volunteer not found.");

            if (volunteer.Password != password)
                throw new BO.InvalidPasswordException("Invalid password.");

            // Return the volunteer's role as a string
            return volunteer.Jobs;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.VolunteerNotFoundException("Volunteer not found.", ex);
        }
    }


    /// <summary>
    /// Retrieves a filtered and sorted list of volunteers.
    /// </summary>
    /// <param name="isActive">Filter for active or inactive volunteers.</param>
    /// <param name="sortBy">Field to sort by.</param>
    /// <returns>A list of BO.VolunteerInList objects.</returns>
    public IEnumerable<BO.VolunteerInList> GetVolunteerList(bool? isActive = null, BO.VolunteerSortField? sortBy = null)
    {
        try
            {
                var volunteers = _dal.Volunteer.ReadAll()
                    .Select(v =>
                    {
                        var currentCallId = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && a.EndType == null)
                            .Select(a => (int?)a.CallId).FirstOrDefault();

                        BO.CallType callType;
                        if (!currentCallId.HasValue)
                        {
                            callType = BO.CallType.None;
                        }
                        else
                        {
                            var call = _dal.Call.Read(currentCallId.Value);
                            callType = call != null ? (BO.CallType)call.CallType : BO.CallType.None;
                        }

                        return new BO.VolunteerInList
                        {
                            Id = v.Id,
                            Name = v.Name,
                            IsActive = v.active,
                            TotalHandledCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && a.EndType == DO.EndType.Cared).Count(),
                            TotalCanceledCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && a.EndType == DO.EndType.AdministratorCancellation).Count(),
                            TotalExpiredCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && a.EndType == DO.EndType.ExpiredCancellation).Count(),
                            CurrentCallId = currentCallId,
                            CallType = callType
                        };
                    });

                if (isActive.HasValue)
                    volunteers = volunteers.Where(v => v.IsActive == isActive);

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
                throw new BO.BlException("An error occurred while retrieving the volunteers.", ex);
            }
        }


    /// <summary>
    /// Retrieves detailed information about a volunteer.
    /// </summary>
    /// <param name="id">The ID of the volunteer.</param>
    /// <returns>A BO.Volunteer object containing detailed information.</returns>
    public BO.Volunteer GetVolunteerDetails(int id)
    {
        try
        {
            var doVolunteer = _dal.Volunteer.Read(id) ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist.");

            var currentAssignment = _dal.Assignment.ReadAll(a => a.VolunteerId == id && a.EndType == null).FirstOrDefault();
            var callInProgress = currentAssignment != null
                ? new BO.CallInProgress
                {
                    Id = currentAssignment.CallId,
                    CallId = currentAssignment.CallId,
                    CallType = (BO.CallType)(_dal.Call.Read(currentAssignment.CallId)?.CallType ?? DO.CallType.None),
                    VerbalDescription = _dal.Call.Read(currentAssignment.CallId)?.VerbalDescription,
                    Address = _dal.Call.Read(currentAssignment.CallId)?.Address,
                    OpeningTime = _dal.Call.Read(currentAssignment.CallId)?.OpeningTime ?? DateTime.MinValue,
                    MaximumTime = _dal.Call.Read(currentAssignment.CallId)?.MaximumTime,
                    EntryTime = currentAssignment.EntryTime,
                    Distance = VolunteerManager.CalculateAirDistance(doVolunteer.Latitude.Value, doVolunteer.Longitude.Value, _dal.Call.Read(currentAssignment.CallId)?.Latitude ?? 0, _dal.Call.Read(currentAssignment.CallId)?.Longitude ?? 0),
                    Status = BO.CallStatus.InTreatment
                }
                : null;

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
                SumOfCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == id && a.EndType == DO.EndType.Cared).Count(),
                SumOfCancellation = _dal.Assignment.ReadAll(a => a.VolunteerId == id && a.EndType == DO.EndType.AdministratorCancellation).Count(),
                SumOfExpiredCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == id && a.EndType == DO.EndType.ExpiredCancellation).Count(),
                CallInProgress = callInProgress
            };
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"An error occurred while retrieving details for volunteer with ID={id}.", ex);
        }
    }

    /// <summary>
    /// Updates the details of a volunteer.
    /// </summary>
    /// <param name="id">The ID of the volunteer.</param>
    /// <param name="volunteer">The updated BO.Volunteer object.</param>
    /// <exception cref="BO.VolunteerNotFoundException">Thrown if the volunteer is not found.</exception>
    public void UpdateVolunteer(int requesterId, BO.Volunteer boVolunteer)
    {
        try
        {
            var requester = _dal.Volunteer.Read(requesterId) ?? throw new BO.BlDoesNotExistException($"Requester with ID={requesterId} does not exist.");

            if (requesterId != boVolunteer.Id && requester.Jobs != DO.Jobs.Administrator)
                throw new BO.BlUnauthorizedAccessException("Only administrators can update other volunteers.");

            // Validate input...
            VolunteerManager.ValidateVolunteer(boVolunteer);

            var doVolunteer = _dal.Volunteer.Read(boVolunteer.Id) ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={boVolunteer.Id} does not exist.");

            var updatedVolunteer = doVolunteer with
            {
                Name = boVolunteer.Name,
                Phone = boVolunteer.Phone,
                Email = boVolunteer.Email,
                Password = boVolunteer.Password,
                Address = boVolunteer.Address,
                Latitude = boVolunteer.Latitude,
                Longitude = boVolunteer.Longitude,
                Jobs = (DO.Jobs)boVolunteer.Jobs,
                active = boVolunteer.IsActive,
                MaxDistance = boVolunteer.MaxDistance,
                DistanceType = (DO.DistanceType)boVolunteer.DistanceType
            };

            _dal.Volunteer.Update(updatedVolunteer);
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"An error occurred while updating volunteer with ID={boVolunteer.Id}.", ex);
        }
    }

    /// <summary>
    /// Deletes a volunteer.
    /// </summary>
    /// <param name="id">The ID of the volunteer to delete.</param>
    /// <exception cref="BO.VolunteerNotFoundException">Thrown if the volunteer is not found.</exception>
    public void DeleteVolunteer(int id)
    {
        try
        {
            var volunteer = _dal.Volunteer.Read(id) ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist.");

            var hasCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == id).Any();
            if (hasCalls)
                throw new BO.BlDeletionImpossibleException($"Cannot delete volunteer with ID={id} as they have associated assignments.");

            _dal.Volunteer.Delete(id);
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"An error occurred while deleting volunteer with ID={id}.", ex);
        }
    }


    /// <summary>
    /// Adds a new volunteer.
    /// </summary>
    /// <param name="volunteer">The BO.Volunteer object to add.</param>
    /// <exception cref="BO.DuplicateVolunteerException">Thrown if a volunteer with the same ID already exists.</exception>
    public void CreateVolunteer(BO.Volunteer boVolunteer)
    {
        try
        {
            VolunteerManager.ValidateVolunteer(boVolunteer);

            var doVolunteer = new DO.Volunteer
            {
                Id = boVolunteer.Id,
                Name = boVolunteer.Name,
                Phone = boVolunteer.Phone,
                Email = boVolunteer.Email,
                Password = boVolunteer.Password,
                Address = boVolunteer.Address,
                Latitude = boVolunteer.Latitude,
                Longitude = boVolunteer.Longitude,
                Jobs = (DO.Jobs)boVolunteer.Jobs,
                active = boVolunteer.IsActive,
                MaxDistance = boVolunteer.MaxDistance,
                DistanceType = (DO.DistanceType)boVolunteer.DistanceType
            };

            _dal.Volunteer.Create(doVolunteer);
        }
        catch (Exception ex)
        {
            throw new BO.BlException("An error occurred while creating a new volunteer.", ex);
        }
    }

    public IVolunteer Volunteer { get; } = new VolunteerImplementation();
}


