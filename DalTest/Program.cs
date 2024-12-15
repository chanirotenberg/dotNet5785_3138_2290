namespace DalTest;
using DO;
using Dal;
using DalApi;

public static class Program
{
    // Static field for the data access layer
    //static readonly IDal s_dal = new DalList(); //stage 2
    static readonly IDal s_dal = new DalXml(); //stage 3

    /// <summary>
    /// Main entry point of the program. Initializes the application and displays the main menu.
    /// </summary>
    public static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Welcome to DalTest Program");
            RunMainMenu(); // Run the main menu
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Displays the main menu and handles the user input for main actions.
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
                    InitializeData(); // Initializes the data
                    break;
                case 2:
                    ManageVolunteersMenu(); // Opens the volunteer management menu
                    break;
                case 3:
                    ManageCallsMenu(); // Opens the calls management menu
                    break;
                case 4:
                    ManageAssignmentsMenu(); // Opens the assignments management menu
                    break;
                case 5:
                    ManageConfigurationMenu(); // Opens the configuration management menu
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
    /// Initializes the data using the Initialization class.
    /// </summary>
    private static void InitializeData()
    {
        try
        {
            Console.WriteLine("Initializing data...");
            Initialization.Do(s_dal); // Calls the Initialization class to populate data
            Console.WriteLine("Data initialized successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing data: {ex.Message}");
        }
    }

    /// <summary>
    /// Menu for managing volunteer entities.
    /// </summary>
    private static void ManageVolunteersMenu()
    {
        ManageEntityMenu<Volunteer>(
            "Volunteers",
            s_dal.Volunteer,
            () => new Volunteer
            {
                Id = PromptInt("Enter Volunteer ID: "),
                Name = PromptString("Enter Name: "),
                Phone = PromptString("Enter Phone: "),
                Email = PromptString("Enter Email: "),
                active = PromptBool("Is Active (true/false): "),
                Password = PromptString("Enter Password: "),
                Address = PromptString("Enter Address: "),
                Latitude = PromptDouble("Enter Latitude: "),
                Longitude = PromptDouble("Enter Longitude: "),
                Jobs = PromptEnum<Jobs>("Enter Job (Worker/Administrator): "),
                MaxDistance = PromptDouble("Enter Max Distance: "),
                DistanceType = PromptEnum<DistanceType>("Enter Distance Type (AirDistance, WalkingDistance, DrivingDistance): ") 
            }
        );
    }


    /// <summary>
    /// Menu for managing call entities.
    /// </summary>
    private static void ManageCallsMenu()
    {
        ManageEntityMenu<Call>( // Handles call entity management
            "Calls",
            s_dal.Call,
            () => new Call
            {
                CallType = PromptEnum<CallType>("Enter Call Type (Transport/PickUp): "), // Prompts for the call type
                VerbalDescription = PromptString("Enter Verbal Description: "), // Prompts for the call's verbal description
                Address = PromptString("Enter Address: "), // Prompts for the call's address
                Latitude = PromptDouble("Enter Latitude: "), // Prompts for the call's latitude
                Longitude = PromptDouble("Enter Longitude: "), // Prompts for the call's longitude
                OpeningTime = PromptDateTime("Enter Opening Time: "), // Prompts for the call's opening time
                MaximumTime = PromptDateTime("Enter Maximum Time: ") // Prompts for the call's maximum time
            }
        );
    }

    /// <summary>
    /// Menu for managing assignment entities.
    /// </summary>
    private static void ManageAssignmentsMenu()
    {
        ManageEntityMenu<Assignment>( // Handles assignment entity management
            "Assignments",
            s_dal.Assignment,
            () => new Assignment
            {
                CallId = PromptInt("Enter Call ID: "), // Prompts for the assignment's call ID
                VolunteerId = PromptInt("Enter Volunteer ID: "), // Prompts for the assignment's volunteer ID
                EntryTime = PromptDateTime("Enter Entry Time: "), // Prompts for the entry time
                ActualEndTime = PromptDateTime("Enter Actual End Time: "), // Prompts for the actual end time
                EndType = PromptEnum<EndType>("Enter End Type (cared/selfCancellation/AdministratorCancellation/ExpiredCancellation): ") // Prompts for the end type
            }
        );
    }

    /// <summary>
    /// Menu for managing configuration settings such as clock.
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
                    Console.WriteLine($"Current Clock: {s_dal.Config?.Clock}"); // Shows the current clock
                    break;
                case 2:
                    if (s_dal.Config != null) s_dal.Config!.Clock = s_dal.Config!.Clock.AddMinutes(1); // Advances the clock by 1 minute
                    Console.WriteLine("Clock advanced by 1 minute.");
                    break;
                case 3:
                    if (s_dal.Config != null) s_dal.Config.Clock = s_dal.Config.Clock.AddHours(1); // Advances the clock by 1 hour
                    Console.WriteLine("Clock advanced by 1 hour.");
                    break;
                case 4:
                    s_dal.Config?.Reset(); // Resets the configuration
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
    /// General menu for managing entities like volunteers, calls, assignments.
    /// </summary>
    private static void ManageEntityMenu<T>(string entityName, ICrud<T> dal, Func<T> createEntity) where T : class
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
                        dal.Create(createEntity()); // Adds a new entity
                        Console.WriteLine($"{entityName} added successfully.");
                        break;
                    case 2:
                        Console.WriteLine(dal.Read(PromptInt("Enter ID: "))); // View an entity by its ID
                        break;
                    case 3:
                        foreach (var item in dal.ReadAll()) // View all entities
                            Console.WriteLine(item);
                        break;
                    case 4:
                        UpdateEntity(dal, entityName); // Update an entity by its ID
                        break;
                    case 5:
                        dal.Delete(PromptInt("Enter ID to delete: ")); // Deletes an entity by its ID
                        Console.WriteLine($"{entityName} deleted successfully.");
                        break;
                    case 6:
                        dal.DeleteAll(); // Deletes all entities
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
    /// Update the specified entity by its ID.
    /// </summary>
    private static void UpdateEntity<T>(ICrud<T> dal, string entityName) where T : class
    {
        int id = PromptInt("Enter ID to update: "); // Prompts for the ID of the entity to update
        var entityToUpdate = dal.Read(id); // Reads the entity by its ID
        if (entityToUpdate == null)
        {
            Console.WriteLine($"{entityName} not found.");
            return;
        }

        Console.WriteLine($"Current Data: {entityToUpdate}"); // Shows the current data of the entity
        foreach (var property in entityToUpdate.GetType().GetProperties()) // Loops through the properties of the entity
        {
            if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)) continue; // Skips the Id field

            Console.Write($"Enter new value for {property.Name} (leave empty to keep current): ");
            string? input = Console.ReadLine(); // Prompts for the new value
            if (!string.IsNullOrWhiteSpace(input)) // If input is provided, update the value
            {
                try
                {
                    var convertedValue = Convert.ChangeType(input, property.PropertyType); // Converts input to the correct type
                    property.SetValue(entityToUpdate, convertedValue); // Sets the new value
                }
                catch
                {
                    Console.WriteLine($"Invalid input for {property.Name}. Keeping the current value.");
                }
            }
        }

        dal.Update(entityToUpdate); // Updates the entity
        Console.WriteLine($"{entityName} updated successfully.");
    }

    /// <summary>
    /// Prompt for integer input from the user.
    /// </summary>
    private static int PromptInt(string prompt)
    {
        Console.Write(prompt); // Prompts for an integer input
        return int.Parse(Console.ReadLine()!); // Parses and returns the integer
    }

    /// <summary>
    /// Prompt for string input from the user.
    /// </summary>
    private static string PromptString(string prompt)
    {
        Console.Write(prompt); // Prompts for a string input
        return Console.ReadLine()!; // Returns the string input
    }

    /// <summary>
    /// Prompt for boolean input from the user.
    /// </summary>
    private static bool PromptBool(string prompt)
    {
        Console.Write(prompt); // Prompts for a boolean input
        return bool.Parse(Console.ReadLine()!); // Parses and returns the boolean value
    }

    /// <summary>
    /// Prompt for double input from the user.
    /// </summary>
    private static double PromptDouble(string prompt)
    {
        Console.Write(prompt); // Prompts for a double input
        return double.Parse(Console.ReadLine()!); // Parses and returns the double value
    }

    /// <summary>
    /// Prompt for DateTime input from the user.
    /// </summary>
    private static DateTime PromptDateTime(string prompt)
    {
        Console.Write(prompt); // Prompts for a DateTime input
        if (!DateTime.TryParse(Console.ReadLine(), out var date)) // Tries to parse the input as DateTime
            throw new FormatException("Invalid date format."); // Throws an exception if the input is not valid
        return date; // Returns the DateTime value
    }

    /// <summary>
    /// Prompt for an enum value from the user.
    /// </summary>
    private static TEnum PromptEnum<TEnum>(string prompt) where TEnum : struct
    {
        Console.Write(prompt); // Prompts for an enum input
        if (!Enum.TryParse(Console.ReadLine(), true, out TEnum result)) // Tries to parse the input as an enum value
            throw new FormatException($"Invalid {typeof(TEnum).Name} value."); // Throws an exception if the value is invalid
        return result; // Returns the enum value
    }
}
