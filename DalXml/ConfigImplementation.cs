
namespace Dal;
using DalApi;

internal class ConfigImplementation : IConfig
{
    /// <summary>
    /// Gets or sets the system clock representing the current date and time.
    /// </summary>
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    /// <summary>
    /// Gets or sets the risk range represented as a TimeSpan.
    /// </summary>
    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }

    /// <summary>
    /// Resets the configuration settings to their default values.
    /// </summary>
    public void Reset()
    {
        Config.Reset();
    }
}
