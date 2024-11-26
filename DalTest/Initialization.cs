namespace DalTest;

using Dal;
using DalApi;
using DO;

public static class Initialization
{
    private static IAssignment? s_dalAssignment;
    private static ICall? s_dalCall;
    private static IVolunteer? s_dalVolunteer;
    private static IConfig? s_dalConfig;
    private static readonly Random s_rand = new();

    /// <summary>
    /// Resets all data and configuration.
    /// </summary>
    private static void ResetData()
    {
        Console.WriteLine("Resetting all data...");
        s_dalConfig?.Reset();        // Reset configuration
        s_dalVolunteer?.DeleteAll(); // Delete all volunteers
        s_dalAssignment?.DeleteAll(); // Delete all assignments
        s_dalCall?.DeleteAll();      // Delete all calls
    }

    /// <summary>
    /// Initializes the volunteer list.
    /// </summary>
    private static readonly List<(string address, double latitude, double longitude)> s_addresses = new()
    {
        ("123 Main St, Cityville", 32.0853, 34.7818),
        ("456 Elm St, Townburg", 31.7683, 35.2137),
        ("789 Oak St, Villagetown", 29.5575, 34.9522)
    };

    /// <summary>
    /// Generates a custom password based on the volunteer's name and phone number.
    /// </summary>
    /// <param name="name">The volunteer's name.</param>
    /// <param name="phone">The volunteer's phone number.</param>
    /// <returns>A custom password.</returns>
    private static string GenerateCustomPassword(string name, string phone)
    {
        string namePart = name.Replace(" ", "").Substring(0, Math.Min(3, name.Length)); // Truncate name to max 3 characters
        string phonePart = phone.Substring(Math.Max(0, phone.Length - 3)); // Truncate phone number to last 3 digits
        return $"{namePart}{phonePart}"; // Combine name and phone parts
    }

    /// <summary>
    /// Initializes volunteers.
    /// </summary>
    private static void CreateVolunteers()
    {
        Console.WriteLine("Initializing Volunteers...");
        string[] volunteerNames = {
            "David Cohen", "Sarah Levi", "Michael Ben-David", "Rachel Gold",
            "Daniel Green", "Emma Brown", "James White", "Sophia Black",
            "Benjamin Blue", "Olivia Silver", "Noah Gray", "Emily Rose",
            "Ethan Adler", "Mia Kaplan", "Jacob Fisher"
        };

        foreach (var name in volunteerNames)
        {
            int id;
            do
                id = s_rand.Next(200000000, 400000000);
            while (s_dalVolunteer!.Read(id) != null);
            string phone = GeneratePhoneNumber();
            string email = $"{name.Replace(" ", "").ToLower()}@gmail.com";
            string password = GenerateCustomPassword(name, phone);
            string address = s_addresses[s_rand.Next(s_addresses.Count)].address;
            bool isActive = s_rand.NextDouble() > 0.2;
            double maxDistance = s_rand.Next(10, 50);
            Jobs job = (name == volunteerNames[0]) ? Jobs.Administrator : Jobs.Worker;

            s_dalVolunteer?.Create(new Volunteer
            {
                Id = id,
                Name = name,
                Phone = phone,
                Email = email,
                active = isActive,
                Password = password,
                Address = address,
                Latitude = s_addresses[s_rand.Next(s_addresses.Count)].latitude,
                Longitude = s_addresses[s_rand.Next(s_addresses.Count)].longitude,
                Jobs = job,
                MaxDistance = maxDistance,
                DistanceType = DistanceType.AirDistance
            });
        }
    }

    /// <summary>
    /// Initializes call descriptions.
    /// </summary>
    private static readonly string[] callDescriptions = new string[]
    {
        "Airport ride, taxi needed at a specific time in the morning",
        "Ride to a family event, group pick-up in the city center",
        "Urgent ride to the hospital, needs immediate attention",
        "Business meeting ride, heading to the industrial area in the north",
        "Ride from a fancy restaurant to a hotel downtown",
        "Shopping trip, taxi ride to the big mall for two hours",
        "Ride to the beach, needs to arrive by early evening",
        "Ride to the north, a stop for refueling along the way",
        "Medical ride, heading to a clinic for an important appointment",
        "Festival ride in the city, picking up a group from two locations"
    };

    /// <summary>
    /// Initializes calls.
    /// </summary>
    private static void CreateCalls()
    {
        Console.WriteLine("Initializing Calls...");
        for (int i = 0; i < 50; i++) // At least 50 calls
        {
            DateTime openingTime = s_dalConfig!.Clock.AddMinutes(s_rand.Next(-1000, -1)); // Opening time before current time
            DateTime? maximumTime = s_rand.NextDouble() > 0.3 ? openingTime.AddMinutes(s_rand.Next(10, 120)) : null;

            // Select random call description
            string description = callDescriptions[s_rand.Next(callDescriptions.Length)];

            // Randomly select call type - 70% will be Transport, 30% PickUp
            CallType callType = s_rand.NextDouble() < 0.7 ? CallType.PickUp : CallType.Transport;

            // Select random address from predefined addresses
            string address = s_addresses[s_rand.Next(s_addresses.Count)].address;
            double latitude = s_addresses[s_rand.Next(s_addresses.Count)].latitude;
            double longitude = s_addresses[s_rand.Next(s_addresses.Count)].longitude;

            s_dalCall?.Create(new Call
            {
                CallType = callType,  // Selected call type
                VerbalDescription = description,
                Address = address,
                Latitude = latitude,
                Longitude = longitude,
                OpeningTime = openingTime,
                MaximumTime = maximumTime
            });
        }
    }

    /// <summary>
    /// Generates a random phone number.
    /// </summary>
    /// <returns>A random phone number.</returns>
    private static string GeneratePhoneNumber()
    {
        return $"050-{s_rand.Next(1000000, 9999999)}";
    }

    /// <summary>
    /// Initializes assignments.
    /// </summary>
    private static void CreateAssignments()
    {
        Console.WriteLine("Initializing Assignments...");
        var volunteers = s_dalVolunteer?.ReadAll() ?? new List<Volunteer>();
        var calls = s_dalCall?.ReadAll().Take(s_dalCall.ReadAll().Count() - 15).ToList() ?? new List<Call>();

        // 20% of volunteers will not have any assignments
        int volunteersWithNoAssignments = (int)(volunteers.Count * 0.2);
        var volunteersWithAssignments = volunteers.Skip(volunteersWithNoAssignments).ToList();

        foreach (var volunteer in volunteersWithAssignments)
        {
            // Random number of assignments for the volunteer: between 5 and 10
            int numberOfAssignments = s_rand.Next(5, 11);

            for (int i = 0; i < numberOfAssignments; i++)
            {
                var call = calls[s_rand.Next(calls.Count)];

                DateTime entryTime = call.OpeningTime.AddMinutes(s_rand.Next(1, 60));
                DateTime? actualEndTime = s_rand.NextDouble() > 0.5 ? entryTime.AddMinutes(s_rand.Next(10, 120)) : null;

                EndType? endType = actualEndTime.HasValue
                    ? (s_rand.NextDouble() < 0.8 ? EndType.cared : EndType.selfCancellation)
                    : EndType.AdministratorCancellation;

                s_dalAssignment?.Create(new Assignment
                {
                    CallId = call.Id, // Existing call ID
                    VolunteerId = volunteer.Id, // Existing volunteer ID
                    ActualEndTime = actualEndTime,
                    EndType = endType
                });
            }
        }

        // Handle calls that were not assigned and passed their maximum time
        //foreach (var call in calls)
        //{
        //    if (call.MaximumTime < DateTime.Now)
        //    {
        //        s_dalAssignment?.Create(new Assignment
        //        {
        //            CallId = call.Id,
        //            VolunteerId = 0, // No volunteer assigned
        //            EntryTime = call.OpeningTime,
        //            ActualEndTime = null,
        //            EndType = EndType.ExpiredCancellation
        //        });
        //    }
        //}
    }

    /// <summary>
    /// Initializes all data (volunteers, calls, assignments).
    /// </summary>
    /// <param name="dalVolunteer">Volunteer DAL.</param>
    /// <param name="dalAssignment">Assignment DAL.</param>
    /// <param name="dalCall">Call DAL.</param>
    /// <param name="dalConfig">Config DAL.</param>
    public static void Do(
        IVolunteer? dalVolunteer,
        IAssignment? dalAssignment,
        ICall? dalCall,
        IConfig? dalConfig)
    {
        s_dalVolunteer = dalVolunteer ?? throw new NullReferenceException("Volunteer DAL cannot be null!");
        s_dalAssignment = dalAssignment ?? throw new NullReferenceException("Assignment DAL cannot be null!");
        s_dalCall = dalCall ?? throw new NullReferenceException("Call DAL cannot be null!");
        s_dalConfig = dalConfig ?? throw new NullReferenceException("Config DAL cannot be null!");

        Console.WriteLine("Resetting configuration and clearing all data...");
        ResetData();

        Console.WriteLine("Initializing data...");
        CreateVolunteers();    // Initialize volunteers
        CreateCalls();         // Initialize calls
        CreateAssignments();   // Initialize assignments
    }
}
