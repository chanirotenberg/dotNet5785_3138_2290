namespace DalTest;

using Dal;
using DalApi;
using DO;

public static class Initialization
{
    private static IDal? s_dal; //stage 2

    private static readonly Random s_rand = new();

    /// <summary>
    /// List of addresses with latitude and longitude for random assignment to volunteers and calls.
    /// </summary>
    private static readonly List<(string address, double latitude, double longitude)> s_addresses = new()
    {
        ("1600 Amphitheatre Parkway, Mountain View, CA, USA", 37.4220, -122.0841),
        ("221B Baker Street, London, NW1 6XE, UK", 51.5238, -0.1586),
        ("Kikar Rabin, Tel Aviv-Yafo, Israel", 32.0809, 34.7806),
        ("5th Avenue, New York, NY, USA", 40.7750, -73.9654),
        ("Eiffel Tower, Champ de Mars, Paris, France", 48.8584, 2.2945),
        ("Brandenburg Gate, Berlin, Germany", 52.5163, 13.3777),
        ("Sagrada Familia, Barcelona, Spain", 41.4036, 2.1744),
        ("Sydney Opera House, Sydney, Australia", -33.8568, 151.2153),
        ("Tokyo Tower, Tokyo, Japan", 35.6586, 139.7454),
        ("Taj Mahal, Agra, India", 27.1751, 78.0421)

    };

    /// <summary>
    /// Generates a custom password for volunteers based on their name and phone number.
    /// </summary>
    /// <param name="name">The volunteer's name.</param>
    /// <param name="phone">The volunteer's phone number.</param>
    /// <returns>A custom password in the format of first 3 characters of name and last 3 digits of phone number.</returns>
    private static string GenerateCustomPassword(string name, string phone)
    {
        string namePart = name.Replace(" ", "").Substring(0, Math.Min(3, name.Length)); // Truncate name to max 3 characters
        string phonePart = phone.Substring(Math.Max(0, phone.Length - 3)); // Truncate phone number to last 3 digits
        return $"{namePart}{phonePart}"; // Combine name and phone parts for password
    }

    /// <summary>
    /// Initializes volunteers by creating a list of volunteer data and assigning unique attributes.
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
                id = GenerateValidIsraeliId();
            while (s_dal!.Volunteer.Read(id) != null);
            string phone = GeneratePhoneNumber();
            string email = $"{name.Replace(" ", "").ToLower()}@gmail.com";
            string password = GenerateCustomPassword(name, phone);
            string address = s_addresses[s_rand.Next(s_addresses.Count)].address;
            bool isActive = s_rand.NextDouble() > 0.2;  // 80% chance of being active
            double maxDistance = s_rand.Next(10, 50);  // Max distance between 10 and 50 km
            Jobs job = (name == volunteerNames[0]) ? Jobs.Administrator : Jobs.Worker;  // First volunteer is Admin

            s_dal?.Volunteer.Create(new Volunteer
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
    /// Generates a valid Israeli ID number within the range of 200,000,000 to 400,000,000,
    /// ensuring it has a correct check digit.
    /// </summary>
    /// <returns>A valid 9-digit Israeli ID number.</returns>
    private static int GenerateValidIsraeliId()
    {
        Random rand = new Random();
        int baseId;

        baseId = rand.Next(20000000, 40000000); // Ensure it's within the correct range    

        int[] idArray = baseId.ToString().Select(c => c - '0').ToArray(); // Convert to digit array

        // Calculate check digit
        int sum = 0;
        for (int i = 0; i < 8; i++)
        {
            int digit = idArray[i] * ((i % 2) + 1); // Multiply by 1 or 2 alternately
            sum += (digit > 9) ? digit - 9 : digit; // Sum the digits if the result is >9
        }

        int checkDigit = (10 - (sum % 10)) % 10; // Compute the final check digit

        // Convert back to integer
        return int.Parse(baseId.ToString() + checkDigit);
    }



    /// <summary>
    /// List of descriptions for calls, such as transport or medical ride requests.
    /// </summary>
    private static readonly string[] callDescriptions = new string[] {
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
    /// Initializes a list of calls with random descriptions, locations, and time ranges.
    /// </summary>
    private static void CreateCalls()
    {
        Console.WriteLine("Initializing Calls...");
        for (int i = 0; i < 50; i++) // At least 50 calls
        {
            DateTime openingTime = s_dal!.Config.Clock.AddMinutes(s_rand.Next(-1000, -1));  // Opening time before the current time
            DateTime? maximumTime = s_rand.NextDouble() > 0.3 ? openingTime.AddMinutes(s_rand.Next(10, 120)) : null;  // Random max time

            // Select random call description
            string description = callDescriptions[s_rand.Next(callDescriptions.Length)];

            // Randomly select call type (70% Transport, 30% PickUp)
            CallType callType = s_rand.NextDouble() < 0.7 ? CallType.PickUp : CallType.Transport;

            // Select random address from predefined list
            string address = s_addresses[s_rand.Next(s_addresses.Count)].address;
            double latitude = s_addresses[s_rand.Next(s_addresses.Count)].latitude;
            double longitude = s_addresses[s_rand.Next(s_addresses.Count)].longitude;

            s_dal?.Call.Create(new Call
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
    /// <returns>A random phone number in the format '050-XXXXXXX'.</returns>
    private static string GeneratePhoneNumber()
    {
        return $"050-{s_rand.Next(1000000, 9999999)}";  // Random phone number generation
    }

    /// <summary>
    /// Initializes assignments by creating random assignments for volunteers to calls.
    /// </summary>
    private static void CreateAssignments()
    {
        Console.WriteLine("Initializing Assignments...");
        var volunteers = s_dal?.Volunteer.ReadAll() ?? new List<Volunteer>();
        var calls = s_dal?.Call.ReadAll().Take(s_dal.Call.ReadAll().Count() - 15).ToList() ?? new List<Call>(); // Removing last 15 calls

        // 20% of volunteers will not have any assignments
        int volunteersWithNoAssignments = (int)(volunteers.Count() * 0.2);
        var volunteersWithAssignments = volunteers.Skip(volunteersWithNoAssignments).ToList();

        foreach (var volunteer in volunteersWithAssignments)
        {
            // Random number of assignments for the volunteer: between 5 and 10
            int numberOfAssignments = s_rand.Next(5, 11);

            for (int i = 0; i < numberOfAssignments; i++)
            {
                var call = calls[s_rand.Next(calls.Count)];

                DateTime entryTime = call.OpeningTime.AddMinutes(s_rand.Next(1, 60));  // זמן כניסה לאחר פתיחת הקריאה
                DateTime? actualEndTime = s_rand.NextDouble() > 0.5 ? entryTime.AddMinutes(s_rand.Next(10, 120)) : null;  // זמן סיום רנדומלי

                // סוג סיום הטיפול
                EndType? endType;

                if (actualEndTime.HasValue) // אם הקריאה נסגרה
                {
                    double chance = s_rand.NextDouble();
                    if (chance < 0.8)
                        endType = EndType.Cared; // 80% טופלה
                    else if (chance < 0.9)
                        endType = EndType.SelfCancellation; // 10% ביטול עצמי
                    else
                        endType = EndType.AdministratorCancellation; // 10% ביטול מנהל
                }
                else if (call.MaximumTime.HasValue && call.MaximumTime < s_dal.Config.Clock) // אם הקריאה עדיין פתוחה אבל עבר זמן הסיום
                {
                    endType = EndType.ExpiredCancellation; // הקריאה פג תוקף
                    actualEndTime = s_dal.Config.Clock; // קובעים את זמן הסיום כרגע
                }
                else
                {
                    endType = null; // עדיין פתוח לטיפול
                }


                s_dal?.Assignment.Create(new Assignment
                {
                    CallId = call.Id, // Existing call ID
                    VolunteerId = volunteer.Id, // Existing volunteer ID
                    ActualEndTime = actualEndTime,
                    EndType = endType
                });
            }
        }
    }

    /// <summary>
    /// Initializes all data by resetting the database and creating volunteers, calls, and assignments.
    /// </summary>
    /// <param name="dal">The IDal instance for accessing data layer methods.</param>
    public static void Do()
    {
        //s_dal = dal ?? throw new NullReferenceException("DAL object can not be null!"); // Ensure DAL is initialized
        s_dal = DalApi.Factory.Get; //stage 4

        Console.WriteLine("Resetting configuration and clearing all data...");
        s_dal.ResetDB();

        Console.WriteLine("Initializing data...");
        CreateVolunteers();    // Initialize volunteers
        CreateCalls();         // Initialize calls
        CreateAssignments();   // Initialize assignments
    }
}
