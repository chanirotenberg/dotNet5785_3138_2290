namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;

internal class CallImplementation : ICall
{
        private readonly DalApi.IDal _dal = DalApi.Factory.Get;

        public int[] GetCallCountsByStatus()
        {
            try
            {
                return _dal.Call.ReadAll()
                    .GroupBy(c => (int)c.Status)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .ToArray();
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to calculate call quantities by status.", ex);
            }
        }

    public IEnumerable<CallInList> GetCallList(CallFilterField? filterField = null, object? filterValue = null, CallSortField? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public Call GetCallDetails(int id)
    {
        throw new NotImplementedException();
    }

    public void UpdateCall(Call call)
    {
        throw new NotImplementedException();
    }

    public void DeleteCall(int id)
    {
        throw new NotImplementedException();
    }

    public void AddCall(Call call)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ClosedCallInList> GetClosedCallsByVolunteer(int volunteerId, CallType? callType = null, CallSortField? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<OpenCallInList> GetOpenCallsForVolunteer(int volunteerId, CallType? callType = null, CallSortField? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public void CloseCall(int volunteerId, int assignmentId)
    {
        throw new NotImplementedException();
    }

    public void CancelCall(int requesterId, int assignmentId)
    {
        throw new NotImplementedException();
    }

    public void AssignCallToVolunteer(int volunteerId, int callId)
    {
        throw new NotImplementedException();
    }

    public ICall Call { get; } = new CallImplementation();
}
