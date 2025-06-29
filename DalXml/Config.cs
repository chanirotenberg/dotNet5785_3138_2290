using System.Runtime.CompilerServices;

namespace Dal;

internal static class Config
{
    /// <summary>
    /// The file name for the configuration XML containing system settings.
    /// </summary>
    internal const string s_data_config_xml = "data-config.xml";

    /// <summary>
    /// The file name for the assignments XML storing assignment data.
    /// </summary>
    internal const string s_assignments_xml = "assignments.xml";

    /// <summary>
    /// The file name for the calls XML storing call data.
    /// </summary>
    internal const string s_calls_xml = "calls.xml";

    /// <summary>
    /// The file name for the volunteers XML storing volunteer data.
    /// </summary>
    internal const string s_volunteers_xml = "volunteers.xml";

    /// <summary>
    /// Gets or sets the next available Call ID by retrieving and incrementing it from the configuration XML.
    /// </summary>
    internal static int NextCallId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextCallId");
        [MethodImpl(MethodImplOptions.Synchronized)]
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextCallId", value);
    }

    /// <summary>
    /// Gets or sets the next available Assignment ID by retrieving and incrementing it from the configuration XML.
    /// </summary>
    internal static int NextAssignmentId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextAssignmentId");
        [MethodImpl(MethodImplOptions.Synchronized)]
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextAssignmentId", value);
    }

    /// <summary>
    /// Gets or sets the system clock representing the current date and time.
    /// </summary>
    /// <remarks>
    /// The Clock value is stored and retrieved from the s_data_config.xml configuration file.
    /// </remarks>
    internal static DateTime Clock
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    /// <summary>
    /// Gets or sets the RiskRange, represented as a TimeSpan, from the configuration XML.
    /// </summary>
    /// <remarks>
    /// RiskRange indicates the time span for risk assessment and is stored in the s_data_config.xml file.
    /// </remarks>
    internal static TimeSpan RiskRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigTimeVal(s_data_config_xml, "RiskRange");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigTimeVal(s_data_config_xml, "RiskRange", value);
    }

    /// <summary>
    /// Resets all configuration settings to their default values.
    /// </summary>
    /// <remarks>
    /// Sets the NextCallId and NextAssignmentId to 1000, initializes the system clock to the current time,
    /// and sets RiskRange to a default of 1 hour
    /// </remarks>
    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Reset()
    {
        NextCallId = 1000;
        NextAssignmentId = 1000;
        Clock = DateTime.Now;
        RiskRange = TimeSpan.FromHours(1);
    }
}
