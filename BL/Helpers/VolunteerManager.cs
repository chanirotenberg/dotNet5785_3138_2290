using DalApi;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Helpers
{
    /// <summary>
    /// Manages volunteer-related operations, including validation and address-based calculations.
    /// </summary>
    internal static class VolunteerManager
    {
        internal static ObserverManager Observers = new(); //stage 5 
        private static readonly DalApi.IDal _dal = DalApi.Factory.Get;


        private static readonly object _lock = new();
        private static readonly Random s_rand = new Random();

        /// <summary>
        /// Validates a volunteer object to ensure correct data input.
        /// </summary>
        /// <param name="volunteer">The volunteer object to validate.</param>
        //public static void ValidateVolunteer(BO.Volunteer volunteer, bool isPartial = false, string? oldPassword = null)
        //{
        //    lock (_lock)
        //    {
        //        if (!isPartial)
        //        {
        //            if (string.IsNullOrWhiteSpace(volunteer.Name) || volunteer.Name.Length < 2)
        //                throw new BO.BlValidationException("Name must be at least 2 characters long.");
        //        }

        //        if (!string.IsNullOrWhiteSpace(volunteer.Email))
        //        {
        //            if (!volunteer.Email.Contains("@") || !volunteer.Email.Contains("."))
        //                throw new BO.BlValidationException("Invalid email format.");
        //        }
        //        else if (!isPartial)
        //        {
        //            throw new BO.BlValidationException("Email is required.");
        //        }

        //        if (volunteer.Password != oldPassword)
        //        {
        //            if (!IsStrongPassword(volunteer.Password, oldPassword))
        //                throw new BO.BlValidationException("Password must be at least 8 characters long, include an uppercase letter, a lowercase letter, a number, and a special character.");
        //            volunteer.Password = EncryptPassword(volunteer.Password);
        //        }

        //        if (!IsValidIsraeliId(volunteer.Id))
        //            throw new BO.BlValidationException("Invalid ID.");

        //        if (!string.IsNullOrWhiteSpace(volunteer.Phone))
        //        {
        //            if (!IsNumeric(volunteer.Phone) || volunteer.Phone.Length != 10)
        //                throw new BO.BlValidationException("Phone number must be numeric and 10 digits long.");
        //        }
        //        else if (!isPartial)
        //        {
        //            throw new BO.BlValidationException("Phone number is required.");
        //        }

        //        if (volunteer.Latitude.HasValue && (volunteer.Latitude < -90 || volunteer.Latitude > 90))
        //            throw new BO.BlValidationException("Latitude must be between -90 and 90.");

        //        if (volunteer.Longitude.HasValue && (volunteer.Longitude < -180 || volunteer.Longitude > 180))
        //            throw new BO.BlValidationException("Longitude must be between -180 and 180.");

        //        if (volunteer.MaxDistance.HasValue && volunteer.MaxDistance <= 0)
        //            throw new BO.BlValidationException("MaxDistance must be positive.");

        //        if (string.IsNullOrWhiteSpace(volunteer.Address))
        //            throw new BO.BlValidationException("Address is required.");
        //    }
        //}
        //public static async Task CompleteCoordinatesAsync(BO.Volunteer volunteer)
        //{
        //    try
        //    {
        //        var (latitude, longitude) = await Tools.GetCoordinatesAsync(volunteer.Address);
        //        volunteer.Latitude = latitude;
        //        volunteer.Longitude = longitude;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new BO.BlValidationException($"Invalid address: {volunteer.Address}. Details: {ex.Message}");
        //    }
        //}

        public static async Task ValidateVolunteerAsync(BO.Volunteer volunteer, bool isPartial = false, string? oldPassword = null)
        {
            lock (_lock)
            {
                if (!isPartial)
                {
                    if (string.IsNullOrWhiteSpace(volunteer.Name) || volunteer.Name.Length < 2)
                        throw new BO.BlValidationException("Name must be at least 2 characters long.");
                }

                if (!string.IsNullOrWhiteSpace(volunteer.Email))
                {
                    if (!volunteer.Email.Contains("@") || !volunteer.Email.Contains("."))
                        throw new BO.BlValidationException("Invalid email format.");
                }
                else
                {
                    throw new BO.BlValidationException("Email is required.");
                }

                if (volunteer.Password != oldPassword)
                {
                    if (!IsStrongPassword(volunteer.Password, oldPassword))
                        throw new BO.BlValidationException("Password must be at least 8 characters long, include an uppercase letter, a lowercase letter, a number, and a special character.");
                    volunteer.Password = EncryptPassword(volunteer.Password);
                }

                if (!IsValidIsraeliId(volunteer.Id))
                    throw new BO.BlValidationException("Invalid ID.");

                if (!string.IsNullOrWhiteSpace(volunteer.Phone))
                {
                    if (!IsNumeric(volunteer.Phone) || volunteer.Phone.Length != 10)
                        throw new BO.BlValidationException("Phone number must be numeric and 10 digits long.");
                }
                else if (!isPartial)
                {
                    throw new BO.BlValidationException("Phone number is required.");
                }

                if (volunteer.Latitude.HasValue && (volunteer.Latitude < -90 || volunteer.Latitude > 90))
                    throw new BO.BlValidationException("Latitude must be between -90 and 90.");

                if (volunteer.Longitude.HasValue && (volunteer.Longitude < -180 || volunteer.Longitude > 180))
                    throw new BO.BlValidationException("Longitude must be between -180 and 180.");

                if (volunteer.MaxDistance.HasValue && volunteer.MaxDistance <= 0)
                    throw new BO.BlValidationException("MaxDistance must be positive.");

                if (string.IsNullOrWhiteSpace(volunteer.Address))
                    throw new BO.BlValidationException("Address is required.");
            }

            // קריאה אסינכרונית מחוץ ל-lock
            try
            {
                var (latitude, longitude) = await Tools.GetCoordinatesAsync(volunteer.Address);
                volunteer.Latitude = latitude;
                volunteer.Longitude = longitude;
            }
            catch (Exception ex)
            {
                throw new BO.BlValidationException($"Invalid address: {volunteer.Address}. Details: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a password is strong.
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <returns>True if the password is strong, otherwise false.</returns>
        private static bool IsStrongPassword(string password, string oldPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

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
        /// Checks if an Israeli ID is valid using checksum validation.
        /// </summary>
        /// <param name="id">The ID to validate.</param>
        /// <returns>True if the ID is valid, otherwise false.</returns>
        private static bool IsValidIsraeliId(int id)
        {
            if (id <= 0)
                return false;
            var idStr = id.ToString("D9");
         
            int sum = 0;
            for (int i = 0; i < idStr.Length; i++)
            {
                int digit = int.Parse(idStr[i].ToString());
                if (i % 2 == 1) digit *= 2;
                sum += digit > 9 ? digit - 9 : digit;
            }
            return sum % 10 == 0;
        }

        /// <summary>
        /// Checks if a given string is numeric.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string is numeric, otherwise false.</returns>
        private static bool IsNumeric(string value)
        {
            return value.All(char.IsDigit);
        }

        internal static async Task SimulateVolunteerActivityAsync()
        {
            List<int> volunteersToNotify = new();
            List<int> assignmentsToNotify = new();

            List<DO.Volunteer> activeVolunteers;

            lock (_lock)
            {
                activeVolunteers = _dal.Volunteer.ReadAll(v => v.active).ToList();
            }

            foreach (var volunteer in activeVolunteers)
            {
                try
                {
                    DO.Assignment? currentAssignment;

                    lock (_lock)
                    {
                        currentAssignment = _dal.Assignment
                            .ReadAll(a => a.VolunteerId == volunteer.Id && a.ActualEndTime == null)
                            .FirstOrDefault();
                    }

                    if (currentAssignment == null)
                    {
                        if (s_rand.NextDouble() < 0.2)
                        {
                            List<BO.OpenCallInList> openCallsForVolunteer;

                            try
                            {
                                openCallsForVolunteer = GetOpenCallsForVolunteer(volunteer.Id).ToList();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to get open calls for volunteer {volunteer.Id}: {ex.Message}");
                                continue;
                            }

                            if (openCallsForVolunteer.Count > 0)
                            {
                                var chosenCall = openCallsForVolunteer[s_rand.Next(openCallsForVolunteer.Count)];
                                AssignCallToVolunteer(volunteer.Id, chosenCall.Id);
                            }
                        }
                    }
                    else
                    {
                        double distanceKm;
                        DO.Call call;

                        lock (_lock)
                        {
                            call = _dal.Call.Read(currentAssignment.CallId);
                        }

                        distanceKm = CallManager.CalculateDistance(volunteer, call.Address);

                        var timeSinceStart = DateTime.Now - currentAssignment.EntryTime;
                        var randomAdditionalTime = TimeSpan.FromMinutes(s_rand.Next(1, 20));
                        var threshold = TimeSpan.FromMinutes(distanceKm * 2) + randomAdditionalTime;

                        if (timeSinceStart >= threshold)
                        {
                            CloseCall(volunteer.Id, currentAssignment.Id);
                        }
                        else
                        {
                            if (s_rand.NextDouble() < 0.1)
                            {
                                CancelCall(volunteer.Id, currentAssignment.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during volunteer simulation for volunteer {volunteer.Id}: {ex.Message}");
                }
            }

            // מחוץ ל־lock: הודעה למשקיפים
            foreach (var assignmentId in assignmentsToNotify)
            {
                Observers.NotifyItemUpdated(assignmentId);
            }

            Observers.NotifyListUpdated();

            foreach (var volunteerId in volunteersToNotify)
            {
                if (SpecificVolunteerObservers.TryGetValue(volunteerId, out var observer))
                {
                    observer?.Invoke();
                }
            }

        }


        public static IEnumerable<BO.OpenCallInList> GetOpenCallsForVolunteer(int volunteerId, BO.CallType? callType = null, BO.CallSortAndFilterField? sortBy = null)
        {
            try
            {
                DO.Volunteer volunteer;

                lock (AdminManager.BlMutex)
                {
                    volunteer = _dal.Volunteer.Read(volunteerId)
                        ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={volunteerId} does not exist.");
                }

                double? maxDistance = volunteer.MaxDistance;

                List<BO.OpenCallInList> calls;

                lock (AdminManager.BlMutex)
                {
                    calls = _dal.Call.ReadAll()
                        .Where(c => CallManager.DetermineCallStatus(c.Id) == 0 || CallManager.DetermineCallStatus(c.Id) == 5)
                        .Where(c => callType == null || (BO.CallType)c.CallType == callType)
                        .Select(c =>
                        {
                            double distance = CallManager.CalculateDistance(volunteer, c.Address);
                            return new BO.OpenCallInList
                            {
                                Id = c.Id,
                                CallType = (BO.CallType)c.CallType,
                                VerbalDescription = c.VerbalDescription,
                                Address = c.Address,
                                OpeningTime = c.OpeningTime,
                                MaximumTime = c.MaximumTime,
                                DistanceFromVolunteer = distance
                            };
                        }).ToList();
                }

                if (maxDistance != null)
                {
                    calls = calls.Where(c => c.DistanceFromVolunteer <= maxDistance.Value).ToList();
                }

                return sortBy.HasValue
                    ? calls.OrderBy(c => c.GetType().GetProperty(sortBy.ToString())?.GetValue(c))
                    : calls.OrderBy(c => c.Id);
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to retrieve open calls for volunteer.", ex);
            }
        }

        public static void CloseCall(int volunteerId, int assignmentId)
        {
            try
            {
                DO.Assignment assignment;

                lock (AdminManager.BlMutex)
                {
                    assignment = _dal.Assignment.Read(assignmentId)
                        ?? throw new BO.BlDoesNotExistException($"Assignment with ID={assignmentId} does not exist.");

                    if (assignment.VolunteerId != volunteerId)
                        throw new BO.BlAuthorizationException($"Volunteer with ID={volunteerId} is not authorized to close this call.");

                    if (assignment.ActualEndTime != null || assignment.EndType != null)
                        throw new BO.BlLogicException($"The assignment with ID={assignmentId} is already closed or expired.");

                    assignment = assignment with
                    {
                        ActualEndTime = _dal.Config.Clock,
                        EndType = DO.EndType.Cared
                    };

                    _dal.Assignment.Update(assignment);
                }

                Observers.NotifyItemUpdated(assignment.VolunteerId);
                Observers.NotifyListUpdated();
                CallManager.Observers.NotifyItemUpdated(volunteerId);
                CallManager.Observers.NotifyListUpdated();
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to close the call.", ex);
            }
        }
        public static void CancelCall(int requesterId, int assignmentId)
        {
            try
            {
                DO.Assignment assignment;

                lock (AdminManager.BlMutex)
                {
                    assignment = _dal.Assignment.Read(assignmentId)
                        ?? throw new BO.BlDoesNotExistException($"Assignment with ID={assignmentId} does not exist.");

                    if (assignment.ActualEndTime != null || assignment.EndType != null)
                        throw new BO.BlLogicException($"The assignment with ID={assignmentId} is already closed or expired.");

                    var isRequesterAuthorized =
                        assignment.VolunteerId == requesterId ||
                        _dal.Volunteer.Read(requesterId)?.Jobs == DO.Jobs.Administrator;

                    if (!isRequesterAuthorized)
                        throw new BO.BlAuthorizationException($"Requester with ID={requesterId} is not authorized to cancel this call.");

                    assignment = assignment with
                    {
                        ActualEndTime = _dal.Config.Clock,
                        EndType = assignment.VolunteerId == requesterId ? DO.EndType.SelfCancellation : DO.EndType.AdministratorCancellation
                    };

                    _dal.Assignment.Update(assignment);
                }

                Observers.NotifyItemUpdated(assignment.VolunteerId);
                Observers.NotifyListUpdated();
                CallManager.Observers.NotifyItemUpdated(requesterId);
                CallManager.Observers.NotifyItemUpdated(assignment.CallId); // ← הוסיפי שורה זו
                CallManager.Observers.NotifyListUpdated();


                if (assignment.EndType == DO.EndType.AdministratorCancellation)
                {
                    DO.Volunteer volunteer;
                    DO.Call call;

                    lock (AdminManager.BlMutex)
                    {
                        volunteer = _dal.Volunteer.Read(assignment.VolunteerId)
                            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID={assignment.VolunteerId} does not exist.");

                        call = _dal.Call.Read(assignment.CallId)
                            ?? throw new BO.BlDoesNotExistException($"Call with ID={assignment.CallId} does not exist.");
                    }

                    CallManager.SendCancellationEmailAsync(volunteer.Email, new BO.Call
                    {
                        Address = call.Address,
                        VerbalDescription = call.VerbalDescription
                    });
                }
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to cancel the call.", ex);
            }
        }

        public static void AssignCallToVolunteer(int volunteerId, int callId)
        {
            try
            {
                DO.Call call;
                bool isCallOpen;
                bool hasExistingAssignments;

                lock (AdminManager.BlMutex)
                {
                    call = _dal.Call.Read(callId)
                        ?? throw new BO.BlDoesNotExistException($"Call with ID={callId} does not exist.");

                    isCallOpen = CallManager.DetermineCallStatus(call.Id) == 0 || CallManager.DetermineCallStatus(call.Id) == 5;
                    if (!isCallOpen)
                        throw new BO.BlLogicException($"Call with ID={callId} is not open for assignment.");

                    hasExistingAssignments = _dal.Assignment.ReadAll(a => a.CallId == callId && a.ActualEndTime == null).Any();
                    if (hasExistingAssignments)
                        throw new BO.BlLogicException($"Call with ID={callId} is already assigned to a volunteer.");

                    var assignment = new DO.Assignment
                    {
                        CallId = callId,
                        VolunteerId = volunteerId,
                        EntryTime = _dal.Config.Clock,
                        ActualEndTime = null,
                        EndType = null
                    };

                    _dal.Assignment.Create(assignment);
                }

                CallManager.Observers.NotifyItemUpdated(volunteerId);
                CallManager.Observers.NotifyListUpdated();
                VolunteerManager.Observers.NotifyItemUpdated(volunteerId);
                VolunteerManager.Observers.NotifyListUpdated();
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to assign call to volunteer.", ex);
            }
        }

        public static readonly Dictionary<int, Action?> SpecificVolunteerObservers = new();

    }
}

