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
            Console.WriteLine("6. Login Volunteer");
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
                            MaxDistance=PromptDouble("Enter Max Distance: "),
                            DistanceType= (DistanceType)PromptEnum<DistanceType>("Enter Distance Type (AirDistance, WalkingDistance, DrivingDistance): "),
                            Jobs = (Jobs)PromptEnum<Jobs>("Enter Job (Volunteer/Administrator): "),
                            IsActive = (bool)PromptBool("Is Active (true/false): ")
                        };
                        s_bl.Volunteer.CreateVolunteer(newVolunteer);
                        Console.WriteLine("Volunteer added successfully.");
                        break;
                    case 2:
                        int viewId = PromptInt("Enter Volunteer ID: ");
                        Console.WriteLine(s_bl.Volunteer.GetVolunteerDetails(viewId));
                        break;
                    case 3:
                        bool? isActive = PromptBool("Filter by active status? (true for active, false for inactive, empty for all): ");
                        var sortField = PromptEnum<VolunteerSortField>("Enter sort field (Id, Name, SumOfCalls, SumOfCancellation, SumOfExpiredCalls) or leave empty for default sorting: ");
                        var volunteers = s_bl.Volunteer.GetVolunteerList(isActive, sortField);

                        Console.WriteLine("\nFiltered & Sorted Volunteers:");
                        foreach (var volunteer in volunteers)
                            Console.WriteLine(volunteer);
                        break;
                    case 4:
                        int updateId = PromptInt("Enter Volunteer ID to update: ");
                        var updatedVolunteer = s_bl.Volunteer.GetVolunteerDetails(updateId);
                        Console.WriteLine("Update fields (leave empty to keep current value):");

                        string? name = PromptString($"Name [{updatedVolunteer.Name}]: ");
                        if (!string.IsNullOrWhiteSpace(name)) updatedVolunteer.Name = name;

                        string? phone = PromptString($"Phone [{updatedVolunteer.Phone}]: ");
                        if (!string.IsNullOrWhiteSpace(phone)) updatedVolunteer.Phone = phone;

                        string? email = PromptString($"Email [{updatedVolunteer.Email}]: ");
                        if (!string.IsNullOrWhiteSpace(email)) updatedVolunteer.Email = email;

                        string? password = PromptString($"Password [{updatedVolunteer.Password ?? "Not Set"}]: ");
                        if (!string.IsNullOrWhiteSpace(password)) updatedVolunteer.Password = password;

                        string? address = PromptString($"Address [{updatedVolunteer.Address ?? "Not Set"}]: ");
                        if (!string.IsNullOrWhiteSpace(address)) updatedVolunteer.Address = address;

                        Jobs? job = PromptEnum<Jobs>($"Job [{updatedVolunteer.Jobs}]: ");
                        if (job.HasValue) updatedVolunteer.Jobs = job.Value;

                        bool? active = PromptBool($"Is Active [{updatedVolunteer.IsActive}]: ");
                        if (active.HasValue) updatedVolunteer.IsActive = active.Value;

                        double? maxDistance = PromptDouble($"Max Distance [{updatedVolunteer.MaxDistance ?? 0}]: ");
                        if (maxDistance.HasValue) updatedVolunteer.MaxDistance = maxDistance.Value;

                        DistanceType? distanceType = PromptEnum<DistanceType>($"Distance Type [{updatedVolunteer.DistanceType}]: ");
                        if (distanceType.HasValue) updatedVolunteer.DistanceType = distanceType.Value;

                        s_bl.Volunteer.UpdateVolunteer(updateId, updatedVolunteer);
                        Console.WriteLine("Volunteer updated successfully.");
                        break;
                    case 5:
                        int deleteId = PromptInt("Enter Volunteer ID to delete: ");
                        s_bl.Volunteer.DeleteVolunteer(deleteId);
                        Console.WriteLine("Volunteer deleted successfully.");
                        break;
                    case 6:
                        int username=0;
                        password = PromptString("Enter Password: ");
                        job = s_bl.Volunteer.Login(username, password);
                        Console.WriteLine($"Login successful. User role: {job}");
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
            Console.WriteLine("4. Update Call");
            Console.WriteLine("5. Delete Call");
            Console.WriteLine("6. Get Calls by Status");
            Console.WriteLine("7. Get Closed Calls by Volunteer");
            Console.WriteLine("8. Get Open Calls for Volunteer");
            Console.WriteLine("9. Assign Call to Volunteer");
            Console.WriteLine("10. Close Cared Call");
            Console.WriteLine("11. Cancel Call");
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
                            CallType = (CallType)PromptEnum<CallType>("Enter Call Type (Transport/PickUp): "),
                            VerbalDescription = PromptString("Enter Verbal Description: "),
                            Address = PromptString("Enter Address: "),
                            OpeningTime = (DateTime)PromptDateTime("Enter Opening Time: "),
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
                        var filterField = PromptEnum<CallSortAndFilterField>("Enter filter field (Id, CallType, OpeningTime, Status) or leave empty for no filtering: ");

                        object? filterValue = null;
                        if (filterField.HasValue)
                        {
                            switch (filterField.Value)
                            {
                                case CallSortAndFilterField.Id:
                                    filterValue = PromptInt("Enter Call ID to filter: ");
                                    break;
                                case CallSortAndFilterField.CallType:
                                    filterValue = PromptEnum<CallType>("Enter Call Type (Transport, PickUp, None): ");
                                    break;
                                case CallSortAndFilterField.OpeningTime:
                                    filterValue = PromptDateTime("Enter Opening Time (MM/dd/yyyy HH:mm): ");
                                    break;
                                case CallSortAndFilterField.Status:
                                    filterValue = PromptEnum<CallStatus>("Enter Call Status (Open, InTreatment, Closed, Expired, InRiskTreatment, OpenInRisk): ");
                                    break;
                            }
                        }
                       
                        var sortField = PromptEnum<CallSortAndFilterField>("Enter sort field (Id, CallType, OpeningTime, Status) or leave empty for default sorting: ");
                                               
                        var calls = s_bl.Call.GetCallList(filterField, filterValue, sortField);
                                               
                        Console.WriteLine("\nFiltered & Sorted Calls:");
                        foreach (var call in calls)
                            Console.WriteLine(call);
                        break;

                    case 4:
                        int updateId = PromptInt("Enter Call ID to update: ");
                        var updatedCall = s_bl.Call.GetCallDetails(updateId);

                        Console.WriteLine("Update fields (leave empty to keep current value):");

                        var newCallType = PromptEnum<CallType>($"Call Type [{updatedCall.CallType}]: ");
                        if (newCallType.HasValue) updatedCall.CallType = newCallType.Value;

                        string? verbalDescription = PromptString($"Verbal Description [{updatedCall.VerbalDescription}]: ");
                        if (verbalDescription != null) updatedCall.VerbalDescription = verbalDescription;

                        string? address = PromptString($"Address [{updatedCall.Address}]: ");
                        if (address != null) updatedCall.Address = address;

                        DateTime? maximumTime = PromptDateTime($"Maximum Time [{updatedCall.MaximumTime}]: ");
                        if (maximumTime.HasValue) updatedCall.MaximumTime = maximumTime.Value;

                        s_bl.Call.UpdateCall(updatedCall);
                        Console.WriteLine("Call updated successfully.");
                        break;

                    case 5:
                        int deleteId = PromptInt("Enter Call ID to delete: ");
                        s_bl.Call.DeleteCall(deleteId);
                        Console.WriteLine("Call deleted successfully.");
                        break;

                    case 6:
                        Console.WriteLine("Call Status Counts:");
                        var statusCounts = s_bl.Call.GetCallCountsByStatus();
                        for (int i = 0; i < statusCounts.Length; i++)
                            Console.WriteLine($"Status {i}: {statusCounts[i]} calls");
                        break;

                    case 7:
                        int volunteerId = PromptInt("Enter Volunteer ID: ");
                        var closedCalls = s_bl.Call.GetClosedCallsByVolunteer(volunteerId);
                        foreach (var call in closedCalls)
                            Console.WriteLine(call);
                        break;

                    case 8:
                        int volunteerId2 = PromptInt("Enter Volunteer ID: ");
                        var openCalls = s_bl.Call.GetOpenCallsForVolunteer(volunteerId2);
                        foreach (var call in openCalls)
                            Console.WriteLine(call);
                        break;

                    case 9:
                        int assignVolunteerId = PromptInt("Enter Volunteer ID: ");
                        int assignCallId = PromptInt("Enter Call ID: ");
                        s_bl.Call.AssignCallToVolunteer(assignVolunteerId, assignCallId);
                        Console.WriteLine($"Call {assignCallId} assigned to volunteer {assignVolunteerId}.");
                        break;

                    case 10:
                        int closeVolunteerId = PromptInt("Enter Volunteer ID: ");
                        int closeAssignmentId = PromptInt("Enter Assignment ID: ");
                        s_bl.Call.CloseCall(closeVolunteerId, closeAssignmentId);
                        Console.WriteLine("Call closed successfully.");
                        break;

                    case 11:
                        int cancelRequesterId = PromptInt("Enter Requester ID: ");
                        int cancelAssignmentId = PromptInt("Enter Assignment ID: ");
                        s_bl.Call.CancelCall(cancelRequesterId, cancelAssignmentId);
                        Console.WriteLine("Call canceled successfully.");
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
            Console.WriteLine("5. Show Current Risk Range");
            Console.WriteLine("6. Set Risk Range");
            Console.WriteLine("0. Back to Main Menu");

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;

            try
            {
                switch (choice)
                {
                    case 1:
                        s_bl.Admin.ResetDB();
                        Console.WriteLine("Database reset successfully.");
                        break;
                    case 2:
                        s_bl.Admin.InitializeDB();
                        Console.WriteLine("Database initialized successfully.");
                        break;
                    case 3:
                        Console.WriteLine($"Current Clock: {s_bl.Admin.GetClock()}");
                        break;
                    case 4:
                        var unit = (TimeUnit)PromptEnum<TimeUnit>("Enter Time Unit (Minute/Hour/Day/Month/Year): ");
                        s_bl.Admin.AdvanceClock(unit);
                        Console.WriteLine("Clock advanced successfully.");
                        break;
                    case 5:
                        Console.WriteLine($"Current Risk Range: {s_bl.Admin.GetRiskRange()}");
                        break;
                    case 6:
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


    private static void HandleException(Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
    }

    /// <summary>
    /// Prompt for integer input from the user.
    /// </summary>
    private static int PromptInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int result))
                return result;

            Console.WriteLine("Invalid input. Please enter a valid integer.");
        }
    }

    /// <summary>
    /// Prompt for string input from the user. Returns null if left empty.
    /// </summary>
    private static string? PromptString(string prompt)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? null : input;
    }


    /// <summary>
    /// Prompt for boolean input from the user.
    /// </summary>
    private static bool? PromptBool(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return null;
            if (bool.TryParse(input, out bool result))
                return result;

            Console.WriteLine("Invalid input. Please enter 'true' or 'false'.");
        }
    }

    /// <summary>
    /// Prompt for double input from the user.
    /// </summary>
    private static double? PromptDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return null;
            if (double.TryParse(input, out double result))
                return result;

            Console.WriteLine("Invalid input. Please enter a valid number.");
        }
    }

    /// <summary>
    /// Prompt for DateTime input from the user. Returns null if left empty.
    /// </summary>
    private static DateTime? PromptDateTime(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (DateTime.TryParse(input, out DateTime result))
                return result;

            Console.WriteLine("Invalid input. Please enter a valid date and time (e.g., 'MM/dd/yyyy HH:mm').");
        }
    }


    /// <summary>
    /// Prompt for a TimeSpan input from the user.
    /// </summary>
    private static TimeSpan PromptTimeSpan(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (TimeSpan.TryParse(input, out TimeSpan result))
                return result;

            Console.WriteLine("Invalid input. Please enter a valid time span (hh:mm:ss).");
        }
    }

    /// <summary>
    /// Prompt for an enum value from the user. Returns null if left empty.
    /// </summary>
    private static TEnum? PromptEnum<TEnum>(string prompt) where TEnum : struct
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (Enum.TryParse(input, true, out TEnum result))
                return result;

            Console.WriteLine($"Invalid input. Please enter a valid {typeof(TEnum).Name} value.");
        }
    }
}
