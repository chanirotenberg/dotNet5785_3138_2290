namespace DalTest;


using DalApi;
using DO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

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
    /// Encrypts a password using SHA-256.
    /// </summary>
    /// <param name="password">The password to encrypt.</param>
    /// <returns>The encrypted password as a hexadecimal string.</returns>
    public static string EncryptPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }

    /// <summary>
    /// Generates a strong password in the format: Uppercase letter, 7 random digits, lowercase letter, and a special character.
    /// </summary>
    /// <returns>A strong password.</returns>
    private static string GenerateStrongPassword()
    {
        var random = new Random();

        // Generate one uppercase letter
        char upperCase = (char)random.Next('A', 'Z' + 1);

        // Generate seven random digits
        string digits = string.Concat(Enumerable.Range(0, 7).Select(_ => random.Next(0, 10).ToString()));

        // Generate one lowercase letter
        char lowerCase = (char)random.Next('a', 'z' + 1);

        // Select a random special character
        string specialCharacters = "!@#$%*+?";
        char specialChar = specialCharacters[random.Next(specialCharacters.Length)];

        // Combine all parts into a password
        return EncryptPassword($"{upperCase}{digits}{lowerCase}{specialChar}");
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
            string password = GenerateStrongPassword();
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
            DateTime? maximumTime = s_rand.NextDouble() > 0.3 ? openingTime.AddYears(s_rand.Next(5, 20)) : null;  // Random max time

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
        return $"050{s_rand.Next(1000000, 9999999)}";  // Random phone number generation
    }
    private static void CreateAssignments()
    {
        Console.WriteLine("Initializing Assignments...");
        var volunteers = s_dal?.Volunteer.ReadAll() ?? new List<Volunteer>();
        var calls = s_dal?.Call.ReadAll().Take(s_dal.Call.ReadAll().Count() - 15).ToList() ?? new List<Call>();

        var volunteersList = volunteers.ToList();
        int volunteersWithNoAssignments = (int)(volunteersList.Count * 0.2);
        var volunteersWithAssignments = volunteers.Skip(volunteersWithNoAssignments).ToList();

        var availableCalls = new List<Call>(calls); // קריאות זמינות

        foreach (var volunteer in volunteersWithAssignments)
        {
            int numberOfAssignments = s_rand.Next(5, 11); // 5-10 הקצאות

            for (int i = 0; i < numberOfAssignments && availableCalls.Count > 0; i++)
            {
                var callIndex = s_rand.Next(availableCalls.Count);
                var call = availableCalls[callIndex];
                availableCalls.RemoveAt(callIndex); // מניעת שיוך כפול

                DateTime minEntry = call.OpeningTime.AddMinutes(1);
                DateTime maxEntry = call.MaximumTime ?? s_dal.Config.Clock;
                if (maxEntry <= minEntry)
                    maxEntry = minEntry.AddMinutes(1);
                DateTime entryTime = minEntry.AddMinutes(s_rand.Next((int)(maxEntry - minEntry).TotalMinutes));

                DateTime? actualEndTime = null;
                EndType? endType = null;

                bool alreadyStarted = entryTime <= s_dal.Config.Clock;

                if (alreadyStarted)
                {
                    double endChance = s_rand.NextDouble();

                    if (endChance < 0.5)
                    {
                        // הסתיים בהצלחה
                        actualEndTime = entryTime.AddMinutes(s_rand.Next(10, 120));
                        endType = EndType.Cared;
                    }
                    else if (endChance < 0.6)
                    {
                        actualEndTime = entryTime.AddMinutes(s_rand.Next(5, 30));
                        endType = EndType.SelfCancellation;
                    }
                    else if (endChance < 0.7)
                    {
                        actualEndTime = entryTime.AddMinutes(s_rand.Next(5, 30));
                        endType = EndType.AdministratorCancellation;
                    }
                    else if (call.MaximumTime.HasValue && call.MaximumTime < s_dal.Config.Clock)
                    {
                        actualEndTime = s_dal.Config.Clock;
                        endType = EndType.ExpiredCancellation;
                    }
                    // אחרת: השאר את ההקצאה פתוחה
                }
                // אחרת: אם עוד לא התחיל – לא נותנים endType בכלל (השאר פתוח)

                if (endType != null && actualEndTime == null)
                    actualEndTime = s_dal.Config.Clock;

                s_dal?.Assignment.Create(new Assignment
                {
                    CallId = call.Id,
                    VolunteerId = volunteer.Id,
                    EntryTime = entryTime,
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
