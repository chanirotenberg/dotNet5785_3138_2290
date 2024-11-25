namespace Dal;

internal static class DataSource
{
    /// <summary>
    /// A list of all assignments in the system.
    /// </summary>
    internal static List<DO.Assignment> Assignments { get; } = new();

    /// <summary>
    /// A list of all calls in the system.
    /// </summary>
    internal static List<DO.Call> Calls { get; } = new();

    /// <summary>
    /// A list of all volunteers in the system.
    /// </summary>
    internal static List<DO.Volunteer> Volunteers { get; } = new();
}
