namespace BlApi;
/// <summary>
/// Logical service interface for administrative operations.
/// </summary>
public interface IAdmin
{
    /// <summary>
    /// Requests the current system clock value.
    /// </summary>
    /// <returns>The current DateTime value of the system clock.</returns>
    DateTime GetClock();

    /// <summary>
    /// Advances the system clock by a specified time unit.
    /// </summary>
    /// <param name="unit">The time unit to advance (Minute, Hour, Day, Month, Year).</param>
    void AdvanceClock(BO.TimeUnit unit);

    /// <summary>
    /// Requests the current risk time span configuration.
    /// </summary>
    /// <returns>The current TimeSpan value of the risk range.</returns>
    TimeSpan GetRiskRange();

    /// <summary>
    /// Sets a new risk time span configuration.
    /// </summary>
    /// <param name="riskRange">The new TimeSpan value to set for the risk range.</param>
    void SetRiskRange(TimeSpan riskRange);

    /// <summary>
    /// Resets the entire database to initial configuration.
    /// </summary>
    void ResetDatabase();

    /// <summary>
    /// Initializes the database by resetting it and adding initial data.
    /// </summary>
    void InitializeDatabase();
}