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
                else if (!isPartial)
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
            var idStr = id.ToString("D9");
            if (idStr.Length < 9)
                return false; // Pad to 9 digits
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

        /// <summary>
        /// Checks if a given string represents a positive integer.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string is a positive integer, otherwise false.</returns>
        private static bool IsPositiveInteger(string value)
        {
            return int.TryParse(value, out int result) && result > 0;
        }
        internal static async Task SimulateVolunteerActivityAsync()
        {
            // סימולציה שתרוץ בלולאה חזרה (כל שניה) - קריאה חיצונית ממומשת מחוץ
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
                    DO.Assignment? currentAssignment = null;
                    lock (_lock)
                    {
                        currentAssignment = _dal.Assignment.ReadAll(a => a.VolunteerId == volunteer.Id && a.ActualEndTime == null)
                            .FirstOrDefault();
                    }

                    if (currentAssignment == null)
                    {
                        // אין קריאה בטיפול - בסבירות ~20% נבחר קריאה פתוחה לטיפול
                        if (s_rand.NextDouble() < 0.2)
                        {
                            List<BO.Call> openCalls;
                            lock (_lock)
                            {
                                openCalls = _dal.Call.ReadAll(c =>
                                    c.MaximumTime > DateTime.Now &&
                                    c.Latitude != 0 &&
                                    c.Longitude != 0
                                )
                                .Select(c => new BO.Call
                                {
                                    Id = c.Id,
                                    CallType = (BO.CallType)c.CallType,
                                    VerbalDescription = c.VerbalDescription,
                                    Address = c.Address,
                                    Latitude = c.Latitude,
                                    Longitude = c.Longitude,
                                    OpeningTime = c.OpeningTime,
                                    MaximumTime = c.MaximumTime,
                                    Status = (BO.CallStatus)CallManager.DetermineCallStatus(c.Id),
                                    Assignments = null // סימולציה – אין צורך ברשימת שיוכים, ואם כן תוכל למלא כשתצטרך
                                })
                                .ToList();
                            }



                            // סינון קריאות בתוך טווח המתנדב (לפי מרחק, כפי שמוגדר בלוגיקה קיימת)
                            var possibleCalls = openCalls
                                .Where(call =>
                                {
                                    double dist = CallManager.CalculateDistance(volunteer, call.Address);
                                    return dist <= volunteer.MaxDistance;
                                })
                                .ToList();

                            if (possibleCalls.Count > 0)
                            {
                                var chosenCall = possibleCalls[s_rand.Next(possibleCalls.Count)];

                                DO.Assignment newAssignment = new DO.Assignment
                                {
                                    CallId = chosenCall.Id,
                                    VolunteerId = volunteer.Id,
                                    EntryTime = DateTime.Now,
                                    ActualEndTime = null,
                                    EndType = null
                                };

                                lock (_lock)
                                {
                                    _dal.Assignment.Create(newAssignment);
                                }

                                assignmentsToNotify.Add(newAssignment.Id);
                                volunteersToNotify.Add(volunteer.Id);
                            }
                        }
                    }
                    else
                    {
                        // למתנדב יש קריאה בטיפול
                        // בודקים האם "עבר מספיק זמן" לסגור את הקריאה או לבטל

                        // חישוב זמן טיפול אקראי + מרחק בין מתנדב לקריאה
                        double distanceKm;
                        lock (_lock)
                        {
                            var call = _dal.Call.Read(currentAssignment.CallId);
                            distanceKm = CallManager.CalculateDistance(volunteer, call.Address);
                        }

                        var timeSinceStart = DateTime.Now - currentAssignment.EntryTime;
                        var randomAdditionalTime = TimeSpan.FromMinutes(s_rand.Next(1, 20));
                        var threshold = TimeSpan.FromMinutes(distanceKm * 2) + randomAdditionalTime;

                        if (timeSinceStart >= threshold)
                        {
                            // סוגרים את הקריאה כסיימו טיפול
                            var updatedAssignment = currentAssignment with
                            {
                                ActualEndTime = DateTime.Now,
                                EndType = DO.EndType.Cared
                            };

                            lock (_lock)
                            {
                                _dal.Assignment.Update(updatedAssignment);
                            }

                            assignmentsToNotify.Add(updatedAssignment.Id);
                            volunteersToNotify.Add(volunteer.Id);
                        }
                        else
                        {
                            // בהסתברות 10% מבטלים את הטיפול
                            if (s_rand.NextDouble() < 0.1)
                            {
                                var updatedAssignment = currentAssignment with
                                {
                                    ActualEndTime = DateTime.Now,
                                    EndType = DO.EndType.SelfCancellation
                                };

                                lock (_lock)
                                {
                                    _dal.Assignment.Update(updatedAssignment);
                                }

                                assignmentsToNotify.Add(updatedAssignment.Id);
                                volunteersToNotify.Add(volunteer.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // טיפול שגיאות לפי צורך, למשל רישום לוג
                    Console.WriteLine($"Error during volunteer simulation for volunteer {volunteer.Id}: {ex.Message}");
                }
            }

            // מחוץ ל-lock: להודיע למשקיפים על העדכונים
            foreach (var assignmentId in assignmentsToNotify)
            {
                Observers.NotifyItemUpdated(assignmentId);
            }

            foreach (var volunteerId in volunteersToNotify)
            {
                if (SpecificVolunteerObservers.TryGetValue(volunteerId, out var observer))
                {
                    observer?.Invoke();
                }
            }

            // אפשר להוסיף הודעה כללית אם יש צורך
            Observers.NotifyListUpdated();
        }

        public static readonly Dictionary<int, Action?> SpecificVolunteerObservers = new();

    }
}

