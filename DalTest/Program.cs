using Dal;
using DalApi;
using DO;

namespace DalTest;

public static class Program
{
    // Static fields for interfaces
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
    /// Main menu
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
                    ManageVolunteersMenu();
                    break;
                case 3:
                    ManageCallsMenu();
                    break;
                case 4:
                    ManageAssignmentsMenu();
                    break;
                case 5:
                    ManageConfigurationMenu();
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
    /// Initializes data
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
    /// Volunteers management menu
    /// </summary>
    private static void ManageVolunteersMenu()
    {

        if (s_dalVolunteer == null)
        {
            Console.WriteLine("Volunteer DAL is not initialized.");
            return;
        }

        ManageEntityMenu(
            "Volunteers",
            s_dalVolunteer,
            () => new Volunteer
            {
                Id = PromptInt("Enter Volunteer ID: "),
                Name = PromptString("Enter Name: "),
                Phone = PromptString("Enter Phone: "),
                Email = PromptString("Enter Email: "),
                active = PromptBool("Is Active (true/false): "),
                Password = PromptString("Enter Password: "),
                MaxDistance = PromptDouble("Enter Max Distance: ")
            }
        );
    }

    /// <summary>
    /// Calls management menu
    /// </summary>
    private static void ManageCallsMenu()
    {
        if (s_dalCall == null)
        {
            Console.WriteLine("Call DAL is not initialized.");
            return;
        }

        ManageEntityMenu(
            "Calls",
            s_dalCall,
            () => new Call
            {
                CallType = PromptEnum<CallType>("Enter Call Type (Transport/PickUp): "),
                VerbalDescription = PromptString("Enter Verbal Description: "),
                Address = PromptString("Enter Address: "),
                Latitude = PromptDouble("Enter Latitude: "),
                Longitude = PromptDouble("Enter Longitude: "),
                OpeningTime = PromptDateTime("Enter Opening Time: "),
                MaximumTime = PromptDateTime("Enter Maximum Time: ")
            }
        );
    }

    /// <summary>
    /// Assignments management menu
    /// </summary>
    private static void ManageAssignmentsMenu()
    {

        if (s_dalAssignment == null)
        {
            Console.WriteLine("Assignment DAL is not initialized.");
            return;
        }

        ManageEntityMenu(
            "Assignments",
            s_dalAssignment,
            () => new Assignment
            {
                CallId = PromptInt("Enter Call ID: "),
                VolunteerId = PromptInt("Enter Volunteer ID: "),
                EntryTime = PromptDateTime("Enter Entry Time: "),
                ActualEndTime = PromptDateTime("Enter Actual End Time: "),
                EndType = PromptEnum<EndType>("Enter End Type (cared/selfCancellation/AdministratorCancellation/ExpiredCancellation): ")
            }
        );
    }

    /// <summary>
    /// Configuration management menu
    /// </summary>
    private static void ManageConfigurationMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Configuration Menu ---");
            Console.WriteLine("1. Show Current Clock");
            Console.WriteLine("2. Advance Clock by 1 Minute");
            Console.WriteLine("3. Advance Clock by 1 Hour");
            Console.WriteLine("4. Reset Configuration");
            Console.WriteLine("0. Back to Main Menu");

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;

            switch (choice)
            {
                case 1:
                    Console.WriteLine($"Current Clock: {s_dalConfig?.Clock}");
                    break;
                case 2:
                    if (s_dalConfig != null) s_dalConfig.Clock = s_dalConfig.Clock.AddMinutes(1);
                    Console.WriteLine("Clock advanced by 1 minute.");
                    break;
                case 3:
                    if (s_dalConfig != null) s_dalConfig.Clock = s_dalConfig.Clock.AddHours(1);
                    Console.WriteLine("Clock advanced by 1 hour.");
                    break;
                case 4:
                    s_dalConfig?.Reset();
                    Console.WriteLine("Configuration reset.");
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
    /// General entity management menu
    /// </summary>
    private static void ManageEntityMenu<T>(string entityName, dynamic dal, Func<T> createEntity) where T : class
    {
        while (true)
        {
            Console.WriteLine($"\n--- {entityName} Menu ---");
            Console.WriteLine("1. Add Entity");
            Console.WriteLine("2. View Entity by ID");
            Console.WriteLine("3. View All Entities");
            Console.WriteLine("4. Update Entity");
            Console.WriteLine("5. Delete Entity");
            Console.WriteLine("6. Delete All Entities");
            Console.WriteLine("0. Back to Main Menu");

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;

            try
            {
                switch (choice)
                {
                    case 1:
                        dal.Create(createEntity());
                        Console.WriteLine($"{entityName} added successfully.");
                        break;
                    case 2:
                        Console.WriteLine(dal.Read(PromptInt("Enter ID: ")));
                        break;
                    case 3:
                        foreach (var item in dal.ReadAll())
                            Console.WriteLine(item);
                        break;
                    case 4:
                        int id = PromptInt("Enter ID to update: ");
                        var entityToUpdate = dal.Read(id);
                        if (entityToUpdate == null)
                        {
                            Console.WriteLine($"{entityName} not found.");
                            break;
                        }
                        Console.WriteLine($"Current Data: {entityToUpdate}");
                        dal.Update(createEntity());
                        Console.WriteLine($"{entityName} updated successfully.");
                        break;
                    case 5:
                        dal.Delete(PromptInt("Enter ID to delete: "));
                        Console.WriteLine($"{entityName} deleted successfully.");
                        break;
                    case 6:
                        dal.DeleteAll();
                        Console.WriteLine("All entities deleted successfully.");
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error managing {entityName}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Helper method for reading an integer input
    /// </summary>
    private static int PromptInt(string prompt)
    {
        Console.Write(prompt);
        return int.Parse(Console.ReadLine()!);
    }

    private static string PromptString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()!;
    }

    private static bool PromptBool(string prompt)
    {
        Console.Write(prompt);
        return bool.Parse(Console.ReadLine()!);
    }

    private static double PromptDouble(string prompt)
    {
        Console.Write(prompt);
        return double.Parse(Console.ReadLine()!);
    }

    private static DateTime PromptDateTime(string prompt)
    {
        Console.Write(prompt);
        if (!DateTime.TryParse(Console.ReadLine(), out var date))
            throw new FormatException("Invalid date format.");
        return date;
    }

    private static TEnum PromptEnum<TEnum>(string prompt) where TEnum : struct
    {
        Console.Write(prompt);
        if (!Enum.TryParse(Console.ReadLine(), true, out TEnum result))
            throw new FormatException($"Invalid {typeof(TEnum).Name} value.");
        return result;
    }
}
