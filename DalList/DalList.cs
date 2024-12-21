namespace Dal
{
    using DalApi;

    sealed internal class DalList : IDal
    {
        // Static property for Singleton instance (Thread-Safe and Lazy Initialization)
        private static readonly Lazy<DalList> lazyInstance = new(() => new DalList());

        /// <summary>
        /// Gets the single instance of the DalList class.
        /// </summary>
        public static IDal Instance => lazyInstance.Value;

        // Private constructor to prevent instantiation
        private DalList() { }

        public IAssignment Assignment { get; } = new AssignmentImplementation();
        public ICall Call { get; } = new CallImplementation();
        public IVolunteer Volunteer { get; } = new VolunteerImplementation();
        public IConfig Config { get; } = new ConfigImplementation();

        /// <summary>
        /// Resets the database by deleting all entities and resetting configuration.
        /// </summary>
        public void ResetDB()
        {
            Volunteer.DeleteAll();    // Deletes all volunteers
            Call.DeleteAll();         // Deletes all calls
            Assignment.DeleteAll();   // Deletes all assignments
            Config.Reset();           // Resets the configuration
        }
    }
}
