namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;

internal class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;
    public void AddVolunteer(Volunteer volunteer)
    {
        throw new NotImplementedException();
    }

    public void DeleteVolunteer(int id)
    {
        throw new NotImplementedException();
    }

    public Volunteer GetVolunteerDetails(int id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<VolunteerInList> GetVolunteerList(bool? isActive = null, VolunteerSortField? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public string Login(string username, string password)
    {
        throw new NotImplementedException();
    }

    public void UpdateVolunteer(int id, Volunteer volunteer)
    {
        throw new NotImplementedException();
    }
    public  IVolunteer Volunteer { get; } = new VolunteerImplementation();
}

