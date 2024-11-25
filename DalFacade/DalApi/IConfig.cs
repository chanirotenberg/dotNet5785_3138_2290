namespace DalApi;

/// <summary>
/// Interface defining configuration settings and operations.
/// </summary>
public interface IConfig
{
    /// <summary>
    /// Gets or sets the clock representing the current date and time.
    /// </summary>
    DateTime Clock { get; set; }

    /// <summary>
    /// Gets or sets the range of risk represented as a TimeSpan.
    /// </summary>
    TimeSpan RiskRange { get; set; }

    /// <summary>
    /// Resets the configuration settings to their default values.
    /// </summary>
    void Reset();
}
