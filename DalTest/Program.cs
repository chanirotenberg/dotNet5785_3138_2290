using DalApi;
using DO;
using System;

namespace DalTest;

public static class Program
{
    // שדות סטטיים עבור ממשקי הנתונים
    private static IVolunteer? s_dalVolunteer = new VolunteerImplementation();
    private static IAssignment? s_dalAssignment = new AssignmentImplementation();
    private static ICall? s_dalCall = new CallImplementation();
    private static IConfig? s_dalConfig = new ConfigImplementation();

    public static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Welcome to DalTest Program");
            RunMainMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// הצגת תפריט ראשי בלולאה
    /// </summary>
    private static void RunMainMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Initialize Data");
            Console.WriteLine("2. Manage Volunteers");
            Console.WriteLine("3. Manage Calls");
            Console.WriteLine("4. Manage Assignments");
            Console.WriteLine("5. Manage Configuration");
            Console.WriteLine("0. Exit");

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;

            switch (choice)
            {
                case 1:
                    InitializeData();
                    break;
                case 2:
                    RunEntityMenu("Volunteers", ManageVolunteers);
                    break;
                case 3:
                    RunEntityMenu("Calls", ManageCalls);
                    break;
                case 4:
                    RunEntityMenu("Assignments", ManageAssignments);
                    break;
                case 5:
                    ManageConfiguration();
                    break;
                case 0:
                    Console.WriteLine("Exiting program. Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            }
        }
    }

    /// <summary>
    /// אתחול בסיס נתונים
    /// </summary>
    private static void InitializeData()
    {
        try
        {
            Console.WriteLine("Initializing data...");
            Initialization.Do(s_dalVolunteer, s_dalAssignment, s_dalCall, s_dalConfig);
            Console.WriteLine("Data initialized successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing data: {ex.Message}");
        }
    }

    /// <summary>
    /// ניהול תפריט ישויות
    /// </summary>
    private static void RunEntityMenu(string entityName, Action manageEntityAction)
    {
        try
        {
            Console.WriteLine($"\n--- Manage {entityName} ---");
            manageEntityAction();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error managing {entityName}: {ex.Message}");
        }
    }

    /// <summary>
    /// תת-תפריט לניהול מתנדבים
    /// </summary>
    private static void ManageVolunteers()
    {
        while (true)
        {
            Console.WriteLine("\n--- Volunteers Menu ---");
            Console.WriteLine("1. Add Volunteer");
            Console.WriteLine("2. View Volunteer by ID");
            Console.WriteLine("3. View All Volunteers");
            Console.WriteLine("4. Update Volunteer");
            Console.WriteLine("5. Delete Volunteer");
            Console.WriteLine("6. Delete All Volunteers");
            Console.WriteLine("0. Back to Main Menu");

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;

            switch (choice)
            {
                case 1:
                    AddVolunteer();
                    break;
                case 2:
                    ViewVolunteerById();
                    break;
                case 3:
                    ViewAllVolunteers();
                    break;
                case 4:
                    UpdateVolunteer();
                    break;
                case 5:
                    DeleteVolunteer();
                    break;
                case 6:
                    DeleteAllVolunteers();
                    break;
                case 0:
                    return;
                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            }
        }
    }

    /// <summary>
    /// הוספת מתנדב
    /// </summary>
    private static void AddVolunteer()
    {
        try
        {
            Console.WriteLine("Enter Volunteer Details:");
            Console.Write("ID: ");
            int id = int.Parse(Console.ReadLine()!);
            Console.Write("Name: ");
            string name = Console.ReadLine()!;
            Console.Write("Phone: ");
            string phone = Console.ReadLine()!;
            Console.Write("Email: ");
            string email = Console.ReadLine()!;
            Console.Write("Active (true/false): ");
            bool active = bool.Parse(Console.ReadLine()!);

            Volunteer volunteer = new Volunteer
            {
                Id = id,
                Name = name,
                Phone = phone,
                Email = email,
                active = active
            };

            s_dalVolunteer?.Create(volunteer);
            Console.WriteLine("Volunteer added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding volunteer: {ex.Message}");
        }
    }

    /// <summary>
    /// הצגת מתנדב לפי מזהה
    /// </summary>
    private static void ViewVolunteerById()
    {
        try
        {
            Console.Write("Enter Volunteer ID: ");
            int id = int.Parse(Console.ReadLine()!);
            Volunteer? volunteer = s_dalVolunteer?.Read(id);

            if (volunteer != null)
                Console.WriteLine(volunteer);
            else
                Console.WriteLine("Volunteer not found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error viewing volunteer: {ex.Message}");
        }
    }

    /// <summary>
    /// הצגת כל המתנדבים
    /// </summary>
    private static void ViewAllVolunteers()
    {
        try
        {
            foreach (var volunteer in s_dalVolunteer?.ReadAll() ?? new List<Volunteer>())
                Console.WriteLine(volunteer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error viewing volunteers: {ex.Message}");
        }
    }

    /// <summary>
    /// מחיקת כל המתנדבים
    /// </summary>
    private static void DeleteAllVolunteers()
    {
        try
        {
            s_dalVolunteer?.DeleteAll();
            Console.WriteLine("All volunteers deleted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting volunteers: {ex.Message}");
        }
    }
}

