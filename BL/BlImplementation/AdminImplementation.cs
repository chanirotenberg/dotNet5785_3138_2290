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
            return ClockManager.Now;
        }
        /// <summary>
        /// Advances the system clock by a specified time unit.
        /// </summary>
        /// <param name="unit">The time unit to advance (Minute, Hour, Day, Month, Year).</param>
        public void AdvanceClock(BO.TimeUnit unit)
        {
            try
            {
                DateTime newClock = unit switch
                {
                    BO.TimeUnit.Minute => ClockManager.Now.AddMinutes(1),
                    BO.TimeUnit.Hour => ClockManager.Now.AddHours(1),
                    BO.TimeUnit.Day => ClockManager.Now.AddDays(1),
                    BO.TimeUnit.Month => ClockManager.Now.AddMonths(1),
                    BO.TimeUnit.Year => ClockManager.Now.AddYears(1),
                    _ => throw new BO.BlException("Invalid time unit provided.")
                };
                ClockManager.UpdateClock(newClock);
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
            return _dal.Config.RiskRange;
        }

        /// <summary>
        /// Sets a new risk time span configuration.
        /// </summary>
        /// <param name="riskRange">The new TimeSpan value to set for the risk range.</param>
        public void SetRiskRange(TimeSpan riskRange)
        {
            try
            {
                _dal.Config.RiskRange = riskRange;
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to set the risk range configuration.", ex);
            }
        }

        /// <summary>
        /// Resets the entire database to initial configuration.
        /// </summary>
        public void ResetDatabase()
        {
            try
            {
                _dal.ResetDB();
                ClockManager.UpdateClock(ClockManager.Now);
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to reset the database.", ex);
            }
        }


        /// <summary>
        /// Initializes the database by resetting it and adding initial data.
        /// </summary>
        public void InitializeDatabase()
        {
            try
            {
                ResetDatabase();
                DalTest.Initialization.Do();
                ClockManager.UpdateClock(ClockManager.Now);

            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to initialize the database.", ex);
            }
        }
    }
}
