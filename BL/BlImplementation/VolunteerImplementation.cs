namespace BlImplementation;
using BlApi;
using Helpers;
using System.Xml.Linq;

/// <summary>
/// Implementation of volunteer-related operations in the business logic layer.
/// </summary>
internal class VolunteerImplementation : IVolunteer
{
    /// <summary>
    /// Data access layer instance.
    /// </summary>
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    /// <summary>
    /// Logs a volunteer into the system.
    /// </summary>
    /// <param name="username">The volunteer's username.</param>
    /// <param name="password">The volunteer's password.</param>
    /// <returns>The job type of the logged-in volunteer.</returns>
    public BO.Jobs Login(string username, string password)
    {
        try
        {
            var volunteer = _dal.Volunteer.Read(v => v.Name == username)
                            ?? throw new BO.BlDoesNotExistException($"Volunteer with username '{username}' does not exist.");
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

    /// <summary>
    /// Retrieves a list of volunteers with optional filtering and sorting.
    /// </summary>
    /// <param name="isActive">Filter by active status (true for active, false for inactive, null for all).</param>
    /// <param name="sortBy">Sorting field for volunteers.</param>
    /// <returns>A list of volunteers matching the filter criteria.</returns>
    public IEnumerable<BO.VolunteerInList> GetVolunteerList(bool? isActive = null, BO.VolunteerSortField? sortBy = null)
    {
        try
        {
            var volunteers = _dal.Volunteer.ReadAll()
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
            throw new BO.BlException("An error occurred while retrieving the volunteer list.", ex);
        }
    }

    /// <summary>
    /// Creates a new volunteer and adds them to the system.
    /// </summary>
    /// <param name="boVolunteer">The volunteer object containing the details to be added.</param>
    public void CreateVolunteer(BO.Volunteer boVolunteer)
    {
        try
        {
            // Validate volunteer details
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

            // Create volunteer in the data layer
            _dal.Volunteer.Create(doVolunteer);
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

    /// <summary>
    /// Retrieves detailed information of a specific volunteer.
    /// </summary>
    /// <param name="id">The ID of the volunteer to retrieve.</param>
    /// <returns>The volunteer details.</returns>
    public BO.Volunteer GetVolunteerDetails(int id)
    {
        try
        {
            var doVolunteer = _dal.Volunteer.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist.");

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
                DistanceType = (BO.DistanceType)doVolunteer.DistanceType
            };
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"An error occurred while retrieving details for volunteer with ID={id}.", ex);
        }
    }

    /// <summary>
    /// Deletes a volunteer by ID.
    /// </summary>
    /// <param name="id">The ID of the volunteer to delete.</param>
    public void DeleteVolunteer(int id)
    {
        try
        {
            var volunteer = _dal.Volunteer.Read(id)
                ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={id} does not exist.");

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
    /// Updates an existing volunteer's details.
    /// </summary>
    /// <param name="requesterId">The ID of the requester.</param>
    /// <param name="boVolunteer">The updated volunteer details.</param>
    public void UpdateVolunteer(int requesterId, BO.Volunteer boVolunteer)
    {
        try
        {
            var requester = _dal.Volunteer.Read(requesterId)
                ?? throw new BO.BlDoesNotExistException($"Requester with ID={requesterId} does not exist.");

            if (requesterId != boVolunteer.Id && requester.Jobs != DO.Jobs.Administrator)
                throw new BO.BlAuthorizationException($"Requester with ID={requesterId} is not authorized to update volunteer with ID={boVolunteer.Id}.");
            
            VolunteerManager.ValidateVolunteer(boVolunteer,requester.Password);

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

            _dal.Volunteer.Update(doVolunteer);
        }
        catch (Exception ex)
        {
            throw new BO.BlException($"An error occurred while updating volunteer with ID={boVolunteer.Id}.", ex);
        }
    }
}
