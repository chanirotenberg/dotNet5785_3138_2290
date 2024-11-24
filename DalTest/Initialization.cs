
namespace DalTest;
using DalApi;
//using Dal;

using DO;

public static class Initialization
{
    private static IAssignment? s_dalAssignment; //stage 1
    private static ICall? s_dalCall; //stage 1
    private static IVolunteer? s_dalVolunteer; //stage 1
    private static IConfig? s_dalConfig; //stage 1
    private static readonly Random s_rand = new();



    /// <summary>
    /// איפוס נתונים ותצורה
    /// </summary>
    private static void ResetData()
    {
        Console.WriteLine("Resetting all data...");
        s_dalConfig?.Reset();        // איפוס תצורה
        s_dalVolunteer?.DeleteAll(); // מחיקת כל המתנדבים
        s_dalAssignment?.DeleteAll(); // מחיקת כל ההקצאות
        s_dalCall?.DeleteAll();      // מחיקת כל הקריאות
    }

    /// <summary>
    /// אתחול רשימת מתנדבים
    /// </summary>
    /// 
    // אוסף כתובות רנדומליות (לדוגמה עם קווי אורך ורוחב)
    private static readonly List<(string address, double latitude, double longitude)> s_addresses = new()
    {
            ("123 Main St, Cityville", 32.0853, 34.7818),
            ("456 Elm St, Townburg", 31.7683, 35.2137),
            ("789 Oak St, Villagetown", 29.5575, 34.9522)
    };
    /// <summary>
    /// יוצר סיסמה מותאמת המבוססת על שם או מספר טלפון
    /// </summary>
    /// <param name="name">שם המתנדב</param>
    /// <param name="phone">מספר הטלפון של המתנדב</param>
    /// <returns>סיסמה מותאמת</returns>
    private static string GenerateCustomPassword(string name, string phone)
    {
        // קיצוץ השם למקסימום 3 אותיות ראשונות
        string namePart = name.Replace(" ", "").Substring(0, Math.Min(3, name.Length));

        // קיצור מספר הטלפון לשלוש הספרות האחרונות
        string phonePart = phone.Substring(Math.Max(0, phone.Length - 3));

        // הרכבת הסיסמה
        return $"{namePart}{phonePart}";
    }
    private static void createVolunteers()
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
            bool isActive = s_rand.NextDouble() > 0.2;/////
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
                Jobs=job,
                MaxDistance=maxDistance,
                DistanceType = DistanceType.AirDistance
            });
        }
    }

    // אוסף תיאורי קריאות רנדומליים
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

    // פונקציה לאתחול קריאות
    private static void InitializeCalls()
    {
        Console.WriteLine("Initializing Calls...");
        for (int i = 0; i < 50; i++) // לפחות 50 קריאות
        {
            int id = s_dalConfig?.NextCallId ?? 0;  // ID רנדומלי מתוך התצורה
            DateTime openingTime = DateTime.Now.AddMinutes(s_rand.Next(-1000, -1));  // זמן פתיחה לפני הזמן הנוכחי
            DateTime? maximumTime = s_rand.NextDouble() > 0.5 ? openingTime.AddMinutes(s_rand.Next(10, 120)) : null; // זמן סיום רנדומלי או null

            // בחר תיאור רנדומלי לקריאה
            string description = callDescriptions[s_rand.Next(callDescriptions.Length)];

            // בחירה אקראית של סוג קריאה - 70% יהיה Transport, 30% PickUp
            CallType callType = s_rand.NextDouble() < 0.7 ? CallType.PickUp : CallType.Transport;

            // בחר כתובת רנדומלית מתוך הכתובות שהגדרת
            string address = s_addresses[s_rand.Next(s_addresses.Count)].address;
            double latitude = s_addresses[s_rand.Next(s_addresses.Count)].latitude;
            double longitude = s_addresses[s_rand.Next(s_addresses.Count)].longitude;

            s_dalCall?.Create(new Call
            {
                Id = id,
                CallType = callType,  // סוג הקריאה שנבחר על פי אחוזים
                VerbalDescription = description,
                address = address,
                Latitude = latitude,
                Longitude = longitude,
                OpeningTime = openingTime,
                MaximumTime = maximumTime
            });
        }
    }


    

        /// <summary>
        /// יוצר מספר מזהה ייחודי
        /// </summary>
        /// <returns></returns>
        private static int GenerateUniqueId()
        {
            return s_rand.next(200000000, 400000000); // מזהה בטווח
        }

        /// <summary>
        /// יוצר מספר טלפון רנדומלי
        /// </summary>
        /// <returns></returns>
        private static string GeneratePhoneNumber()
        {
            return $"050-{s_rand.Next(1000000, 9999999)}";
        }

    /// <summary>
    /// יוצר סוג קריאה רנדומלי
    /// </summary>
    private static string GenerateCallType()
    {
        string[] callTypes = { "Medical", "Transport", "Delivery", "Emergency" };
        return callTypes[s_rand.Next(callTypes.Length)];
    }

    // פונקציה לאתחול הקצאות
    private static void InitializeAssignments()
    {
        Console.WriteLine("Initializing Assignments...");

        // רשימות מתנדבים וקריאות
        var volunteers = s_dalVolunteer?.ReadAll() ?? new List<Volunteer>();
        var calls = s_dalCall?.ReadAll() ?? new List<Call>();

        // חלוקה של מתנדבים: 20% לא יקבלו קריאות
        int volunteersWithNoAssignments = (int)(volunteers.Count * 0.2);
        var volunteersWithAssignments = volunteers.Skip(volunteersWithNoAssignments).ToList();

        foreach (var volunteer in volunteersWithAssignments)
        {
            // כמות קריאות למתנדב: בין 1 ל-10
            int numberOfAssignments = s_rand.Next(1, 11);

            for (int i = 0; i < numberOfAssignments; i++)
            {
                // בחירת קריאה רנדומלית
                var call = calls[s_rand.Next(calls.Count)];

                // זמן כניסה לטיפול: לאחר זמן פתיחת הקריאה
                DateTime entryTime = call.OpeningTime.AddMinutes(s_rand.Next(1, 60));

                // זמן סיום: ייתכן null או זמן רנדומלי
                DateTime? actualEndTime = s_rand.NextDouble() > 0.5
                    ? entryTime.AddMinutes(s_rand.Next(10, 120))
                    : null;

                // סוג סיום טיפול
                EndType? endType = actualEndTime.HasValue
                    ? (s_rand.NextDouble() < 0.8 ? EndType.cared : EndType.selfCancellation)
                    : EndType.AdministratorCancellation;

                // יצירת הקצאה
                s_dalAssignment?.Create(new Assignment
                {
                    Id = s_dalConfig?.NextAssignmentId ?? 0, // ID מתוך התצורה
                    CallId = call.Id, // ID של קריאה קיימת
                    VolunteerId = volunteer.Id, // ID של מתנדב קיים
                    EntryTime = entryTime,
                    ActualEndTime = actualEndTime,
                    EndType = endType
                });
            }
        }

        // טיפול בקריאות שלא טופלו ועברו זמן סיום
        foreach (var call in calls)
        {
            if (call.MaximumTime < DateTime.Now)
            {
                // קריאה שלא טופלה ועברה את זמן הסיום
                s_dalAssignment?.Create(new Assignment
                {
                    Id = s_dalConfig?.NextAssignmentId ?? 0,
                    CallId = call.Id,
                    VolunteerId = 0, // לא הוקצה מתנדב
                    EntryTime = call.OpeningTime,
                    ActualEndTime = null,
                    EndType = EndType.ExpiredCancellation
                });
            }
        }
    }
    public static void Do(
    IVolunteer? dalVolunteer,
    IAssignment? dalAssignment,
    ICall? dalCall,
    IConfig? dalConfig)
    {
        // וידוא שממשקי הגישה אינם null
        s_dalVolunteer = dalVolunteer ?? throw new NullReferenceException("Volunteer DAL cannot be null!");
        s_dalAssignment = dalAssignment ?? throw new NullReferenceException("Assignment DAL cannot be null!");
        s_dalCall = dalCall ?? throw new NullReferenceException("Call DAL cannot be null!");
        s_dalConfig = dalConfig ?? throw new NullReferenceException("Config DAL cannot be null!");

        // שלב 1: איפוס נתונים
        Console.WriteLine("Resetting configuration and clearing all data...");
        ResetData();

        // שלב 2: אתחול הרשימות
        Console.WriteLine("Initializing data...");
        InitializeVolunteers();    // אתחול מתנדבים
        InitializeCalls();         // אתחול קריאות
        InitializeAssignments();   // אתחול הקצאות
    }

}