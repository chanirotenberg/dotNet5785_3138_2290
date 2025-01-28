namespace BlTest;

using BlApi;
using BO;

public static class Program
{
    // Static field for the logical layer
    static readonly IBl s_bl = Factory.Get();

    /// <summary>
    /// Main entry point of the program.
    /// </summary>
    public static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Welcome to BlTest Program");
            RunMainMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Displays the main menu for logical layer operations.
    /// </summary>
    private static void RunMainMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Manage Volunteers");
            Console.WriteLine("2. Manage Calls");
            Console.WriteLine("3. Manage Admin");
            Console.WriteLine("0. Exit");

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;

            switch (choice)
            {
                case 1:
                    ManageVolunteersMenu();
                    break;
                case 2:
                    ManageCallsMenu();
                    break;
                case 3:
                    ManageAdminMenu();
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
    /// Menu for managing volunteers.
    /// </summary>
    private static void ManageVolunteersMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Volunteers Menu ---");
            Console.WriteLine("1. Add Volunteer");
            Console.WriteLine("2. View Volunteer Details");
            Console.WriteLine("3. View All Volunteers");
            Console.WriteLine("4. Update Volunteer");
            Console.WriteLine("5. Delete Volunteer");
            Console.WriteLine("0. Back to Main Menu");

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;

            try
            {
                switch (choice)
                {
                    case 1:
                        var newVolunteer = new Volunteer
                        {
                            Id = PromptInt("Enter ID: "),
                            Name = PromptString("Enter Name: "),
                            Phone = PromptString("Enter Phone: "),
                            Email = PromptString("Enter Email: "),
                            Password = PromptString("Enter Password: "),
                            Address = PromptString("Enter Address: "),
                            Latitude = PromptDouble("Enter Latitude: "),
                            Longitude = PromptDouble("Enter Longitude: "),
                            Jobs = PromptEnum<Jobs>("Enter Job (Volunteer/Administrator): "),
                            IsActive = PromptBool("Is Active (true/false): ")
                        };
                        s_bl.Volunteer.CreateVolunteer(newVolunteer);
                        Console.WriteLine("Volunteer added successfully.");
                        break;
                    case 2:
                        int viewId = PromptInt("Enter Volunteer ID: ");
                        Console.WriteLine(s_bl.Volunteer.GetVolunteerDetails(viewId));
                        break;
                    case 3:
                        foreach (var volunteer in s_bl.Volunteer.GetVolunteerList())
                            Console.WriteLine(volunteer);
                        break;
                    case 4: // Update Volunteer
                        int updateId = PromptInt("Enter Volunteer ID to update: ");
                        var updatedVolunteer = s_bl.Volunteer.GetVolunteerDetails(updateId);

                        Console.WriteLine("Update the fields (leave empty to keep current value):");

                        string name = PromptString($"Name [{updatedVolunteer.Name}]: ", true);
                        if (!string.IsNullOrWhiteSpace(name))
                            updatedVolunteer.Name = name;

                        string phone = PromptString($"Phone [{updatedVolunteer.Phone}]: ", true);
                        if (!string.IsNullOrWhiteSpace(phone))
                            updatedVolunteer.Phone = phone;

                        string email = PromptString($"Email [{updatedVolunteer.Email}]: ", true);
                        if (!string.IsNullOrWhiteSpace(email))
                            updatedVolunteer.Email = email;

                        string password = PromptString($"Password [{updatedVolunteer.Password}]: ", true);
                        if (!string.IsNullOrWhiteSpace(password))
                            updatedVolunteer.Password = password;

                        string address = PromptString($"Address [{updatedVolunteer.Address}]: ", true);
                        if (!string.IsNullOrWhiteSpace(address))
                            updatedVolunteer.Address = address;

                        double latitude = PromptDouble($"Latitude [{updatedVolunteer.Latitude}]: ");
                        if (!double.IsNaN(latitude))
                            updatedVolunteer.Latitude = latitude;

                        double longitude = PromptDouble($"Longitude [{updatedVolunteer.Longitude}]: ");
                        if (!double.IsNaN(longitude))
                            updatedVolunteer.Longitude = longitude;

                        s_bl.Volunteer.UpdateVolunteer(updateId, updatedVolunteer);
                        Console.WriteLine("Volunteer updated successfully.");
                        break;
                    case 5:
                        int deleteId = PromptInt("Enter Volunteer ID to delete: ");
                        s_bl.Volunteer.DeleteVolunteer(deleteId);
                        Console.WriteLine("Volunteer deleted successfully.");
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
                HandleException(ex);
            }
        }
    }

    /// <summary>
    /// Menu for managing calls.
    /// </summary>
    private static void ManageCallsMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Calls Menu ---");
            Console.WriteLine("1. Add Call");
            Console.WriteLine("2. View Call Details");
            Console.WriteLine("3. View All Calls");
            Console.WriteLine("4. Delete Call");
            Console.WriteLine("0. Back to Main Menu");

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;

            try
            {
                switch (choice)
                {
                    case 1:
                        var newCall = new Call
                        {
                            CallType = PromptEnum<CallType>("Enter Call Type (Transport/PickUp): "),
                            VerbalDescription = PromptString("Enter Verbal Description: "),
                            Address = PromptString("Enter Address: "),
                            Latitude = PromptDouble("Enter Latitude: "),
                            Longitude = PromptDouble("Enter Longitude: "),
                            OpeningTime = PromptDateTime("Enter Opening Time: "),
                            MaximumTime = PromptDateTime("Enter Maximum Time: ")
                        };
                        s_bl.Call.AddCall(newCall);
                        Console.WriteLine("Call added successfully.");
                        break;
                    case 2:
                        int viewId = PromptInt("Enter Call ID: ");
                        Console.WriteLine(s_bl.Call.GetCallDetails(viewId));
                        break;
                    case 3:
                        foreach (var call in s_bl.Call.GetCallList())
                            Console.WriteLine(call);
                        break;
                    case 4:
                        int deleteId = PromptInt("Enter Call ID to delete: ");
                        s_bl.Call.DeleteCall(deleteId);
                        Console.WriteLine("Call deleted successfully.");
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
                HandleException(ex);
            }
        }
    }

    /// <summary>
    /// Menu for managing admin functionalities.
    /// </summary>
    private static void ManageAdminMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Admin Menu ---");
            Console.WriteLine("1. Reset Database");
            Console.WriteLine("2. Initialize Database");
            Console.WriteLine("3. Show Current Clock");
            Console.WriteLine("4. Advance Clock");
            Console.WriteLine("5. Set Risk Range");
            Console.WriteLine("0. Back to Main Menu");

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;

            try
            {
                switch (choice)
                {
                    case 1:
                        s_bl.Admin.ResetDatabase();
                        Console.WriteLine("Database reset successfully.");
                        break;
                    case 2:
                        s_bl.Admin.InitializeDatabase();
                        Console.WriteLine("Database initialized successfully.");
                        break;
                    case 3:
                        Console.WriteLine($"Current Clock: {s_bl.Admin.GetClock()}");
                        break;
                    case 4:
                        var unit = PromptEnum<TimeUnit>("Enter Time Unit (Minute/Hour/Day/Month/Year): ");
                        s_bl.Admin.AdvanceClock(unit);
                        Console.WriteLine("Clock advanced successfully.");
                        break;
                    case 5:
                        var riskRange = PromptTimeSpan("Enter Risk Range (hh:mm:ss): ");
                        s_bl.Admin.SetRiskRange(riskRange);
                        Console.WriteLine("Risk range updated successfully.");
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
                HandleException(ex);
            }
        }
    }

    /// <summary>
    /// Handles exceptions and prints detailed error messages.
    /// </summary>
    private static void HandleException(Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
    }

    /// <summary>
    /// Prompt for TimeSpan input.
    /// </summary>
    private static TimeSpan PromptTimeSpan(string prompt)
    {
        Console.Write(prompt);
        if (TimeSpan.TryParse(Console.ReadLine(), out TimeSpan result))
            return result;

        throw new FormatException("Invalid TimeSpan format.");
    }

    /// <summary>
    /// Prompt for integer input from the user.
    /// </summary>
    private static int PromptInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int result))
                return result;

            Console.WriteLine("Invalid input. Please enter a valid integer.");
        }
    }

    /// <summary>
    /// Prompt for string input from the user.
    /// </summary>
    private static string PromptString(string prompt, bool allowEmpty = false)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input) || allowEmpty)
                return input ?? string.Empty;

            Console.WriteLine("Invalid input. Please enter a valid string.");
        }
    }

    /// <summary>
    /// Prompt for boolean input from the user.
    /// </summary>
    private static bool PromptBool(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (bool.TryParse(Console.ReadLine(), out bool result))
                return result;

            Console.WriteLine("Invalid input. Please enter 'true' or 'false'.");
        }
    }

    /// <summary>
    /// Prompt for double input from the user.
    /// </summary>
    private static double PromptDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (double.TryParse(Console.ReadLine(), out double result))
                return result;

            Console.WriteLine("Invalid input. Please enter a valid number.");
        }
    }

    /// <summary>
    /// Prompt for DateTime input from the user.
    /// </summary>
    private static DateTime PromptDateTime(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (DateTime.TryParse(Console.ReadLine(), out DateTime result))
                return result;

            Console.WriteLine("Invalid input. Please enter a valid date and time (e.g., 'MM/dd/yyyy HH:mm').");
        }
    }

    /// <summary>
    /// Prompt for an enum value from the user.
    /// </summary>
    private static TEnum PromptEnum<TEnum>(string prompt) where TEnum : struct
    {
        while (true)
        {
            Console.Write(prompt);
            if (Enum.TryParse(Console.ReadLine(), true, out TEnum result))
                return result;

            Console.WriteLine($"Invalid input. Please enter a valid {typeof(TEnum).Name} value.");
        }
    }    

}
