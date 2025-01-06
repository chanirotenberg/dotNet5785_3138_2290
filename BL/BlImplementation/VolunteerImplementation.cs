namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;

//internal class VolunteerImplementation : IVolunteer
//{
//    private readonly DalApi.IDal _dal = DalApi.Factory.Get;
//    public void AddVolunteer(Volunteer volunteer)
//    {
//        throw new NotImplementedException();
//    }

//    public void DeleteVolunteer(int id)
//    {
//        throw new NotImplementedException();
//    }

//    public Volunteer GetVolunteerDetails(int id)
//    {
//        throw new NotImplementedException();
//    }

//    public IEnumerable<VolunteerInList> GetVolunteerList(bool? isActive = null, VolunteerSortField? sortBy = null)
//    {
//        throw new NotImplementedException();
//    }

//    public string Login(string username, string password)
//    {
//        throw new NotImplementedException();
//    }

//    public void UpdateVolunteer(int id, Volunteer volunteer)
//    {
//        throw new NotImplementedException();
//    }
//    public  IVolunteer Volunteer { get; } = new VolunteerImplementation();
//}


namespace BlImplementation;
using BlApi;
using DO;
using System.Xml.Linq;

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
    public string Login(string username, string password)
    {
        try
        {
            var volunteer = _dal.Volunteer.Read(v => v.Name == username)
                            ?? throw new BO.VolunteerNotFoundException("Invalid username or volunteer not found.");

            if (volunteer.Password != password)
                throw new BO.InvalidPasswordException("Invalid password.");

            // Return the volunteer's role as a string
            return volunteer.Jobs.ToString();
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
    public IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActive = null, BO.VolunteerSortField? sortBy = null)
    {
        try
        {
            var volunteers = _dal.Volunteer.ReadAll();
            if (isActive.HasValue)
                volunteers = volunteers.Where(v => v.active == isActive.Value);

            var volunteerList = volunteers.Select(v =>
            {
                var currentAssignment = _dal.Assignment.Read(a => a.VolunteerId == v.Id && a.EntryTime != null && a.ActualEndTime == null);
                var currentCallId = currentAssignment?.CallId;

                return new BO.VolunteerInList
                {
                    Id = v.Id,
                    Name = v.Name,
                    IsActive = v.active,
                    TotalHandledCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && a.EndType == DO.EndType.Cared).Count(),
                    TotalCanceledCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && a.EndType == DO.EndType.AdministratorCancellation).Count(),
                    TotalExpiredCalls = _dal.Assignment.ReadAll(a => a.VolunteerId == v.Id && a.EndType == DO.EndType.ExpiredCancellation).Count(),
                    CurrentCallId = currentCallId,
                    CallType = currentCallId.HasValue
                       ? (BO.CallType)(_dal.Call.Read(currentCallId.Value)?.CallType ?? DO.CallType.None)
                        : BO.CallType.None

                };
            });


            return sortBy switch
            {
                BO.VolunteerSortField.SumOfCalls => volunteerList.OrderBy(v => v.TotalHandledCalls),
                BO.VolunteerSortField.SumOfCancellation => volunteerList.OrderBy(v => v.TotalCanceledCalls),
                BO.VolunteerSortField.SumOfExpiredCalls => volunteerList.OrderBy(v => v.TotalExpiredCalls),            
                _ => volunteerList.OrderBy(v => v.Id)
            };
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.DataAccessException("Error accessing data.", ex);
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
            var doVolunteer = _dal.Volunteer.Read(id) ?? throw new BO.VolunteerNotFoundException("Volunteer not found.");
            var currentAssignment = _dal.Assignment.Read(a => a.VolunteerId == id && a.EntryTime != null && a.ActualEndTime == null);
            var currentCallId = currentAssignment?.CallId;
            return new BO.Volunteer
            {
                Id = doVolunteer.Id,
                Name = doVolunteer.Name,
                Phone = doVolunteer.Phone,
                Email = doVolunteer.Email,
                IsActive = doVolunteer.active,
                CallInProgress = _dal.Assignment.Read(a => a.VolunteerId == id && a.EntryTime != null && a.ActualEndTime == null) is { } assignment
                    ? new BO.CallInProgress
                    {
                        CallId = assignment.CallId,
                        CallType = currentCallId.HasValue
                       ? (BO.CallType)(_dal.Call.Read(currentCallId.Value)?.CallType ?? DO.CallType.None)
                        : BO.CallType.None,
                        Address = _dal.Call.Read(assignment.CallId)?.Address ?? string.Empty,
                        OpeningTime = _dal.Call.Read(assignment.CallId)?.OpeningTime ?? DateTime.MinValue,
                        MaximumTime = _dal.Call.Read(assignment.CallId)?.MaximumTime
                    }
                    : null
            };
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.VolunteerNotFoundException("Volunteer not found.", ex);
        }
    }

    /// <summary>
    /// Updates the details of a volunteer.
    /// </summary>
    /// <param name="id">The ID of the volunteer.</param>
    /// <param name="volunteer">The updated BO.Volunteer object.</param>
    /// <exception cref="BO.VolunteerNotFoundException">Thrown if the volunteer is not found.</exception>
    public void UpdateVolunteer(int id, BO.Volunteer volunteer)
    {
        // Implementation for updating volunteer details
    }

    /// <summary>
    /// Deletes a volunteer.
    /// </summary>
    /// <param name="id">The ID of the volunteer to delete.</param>
    /// <exception cref="BO.VolunteerNotFoundException">Thrown if the volunteer is not found.</exception>
    public void DeleteVolunteer(int id)
    {
        // Implementation for deleting a volunteer
    }

    /// <summary>
    /// Adds a new volunteer.
    /// </summary>
    /// <param name="volunteer">The BO.Volunteer object to add.</param>
    /// <exception cref="BO.DuplicateVolunteerException">Thrown if a volunteer with the same ID already exists.</exception>
    public void AddVolunteer(BO.Volunteer volunteer)
    {
        // Implementation for adding a new volunteer
    }
}


