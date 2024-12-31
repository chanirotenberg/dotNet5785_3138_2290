namespace BlImplementation;
using BlApi;
using BO;
using System;

internal class AdminImplementation : IAdmin
{
    public void AdvanceClock(TimeUnit unit)
    {
        throw new NotImplementedException();
    }

    public DateTime GetClock()
    {
        throw new NotImplementedException();
    }

    public TimeSpan GetRiskRange()
    {
        throw new NotImplementedException();
    }

    public void InitializeDatabase()
    {
        throw new NotImplementedException();
    }

    public void ResetDatabase()
    {
        throw new NotImplementedException();
    }

    public void SetRiskRange(TimeSpan riskRange)
    {
        throw new NotImplementedException();
    }
    public IAdmin Admin { get; } = new AdminImplementation();
}

