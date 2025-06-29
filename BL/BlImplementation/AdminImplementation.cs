using BlApi;
using Helpers;

namespace BlImplementation
{
    internal class AdminImplementation : IAdmin
    {
        private readonly DalApi.IDal _dal = DalApi.Factory.Get;

        /// <summary>
        /// Requests the current system clock value.
        /// </summary>
        /// <returns>The current DateTime value of the system clock.</returns>
        public DateTime GetClock()
        {
            lock (AdminManager.BlMutex)
            {
                return AdminManager.Now;
            }
        }

        /// <summary>
        /// Advances the system clock by a specified time unit.
        /// </summary>
        /// <param name="unit">The time unit to advance (Minute, Hour, Day, Month, Year).</param>
        public void AdvanceClock(BO.TimeUnit unit)
        {
            try
            {
                DateTime newClock;
                lock (AdminManager.BlMutex)
                {
                    newClock = unit switch
                    {
                        BO.TimeUnit.Minute => AdminManager.Now.AddMinutes(1),
                        BO.TimeUnit.Hour => AdminManager.Now.AddHours(1),
                        BO.TimeUnit.Day => AdminManager.Now.AddDays(1),
                        BO.TimeUnit.Month => AdminManager.Now.AddMonths(1),
                        BO.TimeUnit.Year => AdminManager.Now.AddYears(1),
                        _ => throw new BO.BlException("Invalid time unit provided.")
                    };
                    AdminManager.UpdateClock(newClock);
                }
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to advance the system clock.", ex);
            }
        }

        /// <summary>
        /// Requests the current risk time span configuration.
        /// </summary>
        /// <returns>The current TimeSpan value of the risk range.</returns>
        public TimeSpan GetRiskRange()
        {
            lock (AdminManager.BlMutex)
            {
                return AdminManager.RiskRange;
            }
        }

        /// <summary>
        /// Sets a new risk time span configuration.
        /// </summary>
        /// <param name="riskRange">The new TimeSpan value to set for the risk range.</param>
        public void SetRiskRange(TimeSpan riskRange)
        {
            try
            {
                lock (AdminManager.BlMutex)
                {
                    AdminManager.RiskRange = riskRange;
                }
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to set the risk range configuration.", ex);
            }
        }

        /// <summary>
        /// Resets the entire database to initial configuration.
        /// </summary>
        public void ResetDB()
        {
            lock (AdminManager.BlMutex)
            {
                AdminManager.ResetDB();
            }
        }

        /// <summary>
        /// Initializes the database by resetting it and adding initial data.
        /// </summary>
        public void InitializeDB()
        {
            lock (AdminManager.BlMutex)
            {
                AdminManager.InitializeDB();
            }
        }

        #region Stage 5
        public void AddClockObserver(Action clockObserver)
        {
            lock (AdminManager.BlMutex)
            {
                AdminManager.ClockUpdatedObservers += clockObserver;
            }
        }

        public void RemoveClockObserver(Action clockObserver)
        {
            lock (AdminManager.BlMutex)
            {
                AdminManager.ClockUpdatedObservers -= clockObserver;
            }
        }

        public void AddConfigObserver(Action configObserver)
        {
            lock (AdminManager.BlMutex)
            {
                AdminManager.ConfigUpdatedObservers += configObserver;
            }
        }

        public void RemoveConfigObserver(Action configObserver)
        {
            lock (AdminManager.BlMutex)
            {
                AdminManager.ConfigUpdatedObservers -= configObserver;
            }
        }
        #endregion Stage 5

        public void StartSimulator(int interval)  //stage 7
        {
            lock (AdminManager.BlMutex)
            {
                AdminManager.ThrowOnSimulatorIsRunning();
                AdminManager.Start(interval);
            }
        }

        public void StopSimulator()
        {
            lock (AdminManager.BlMutex)
            {
                AdminManager.Stop();
            }
        }
    }
}
