using System.Net.Mail;
using System.Net;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using BO;

namespace Helpers
{
    internal static class CallManager
    {
        internal static ObserverManager Observers = new();
        private static readonly DalApi.IDal _dal = DalApi.Factory.Get;
        private static readonly object _lock = new();

        public static int DetermineCallStatus(int callId)
        {
            lock (_lock)
            {
                var now = _dal.Config.Clock;
                var riskRange = _dal.Config.RiskRange;
                var call = _dal.Call.Read(callId) ?? throw new BlDoesNotExistException($"Call with ID {callId} does not exist.");
                var assignments = _dal.Assignment.ReadAll(a => a.CallId == callId).OrderByDescending(a => a.EntryTime).ToList();

                if (assignments.Any())
                {
                    var latestAssignment = assignments.First();

                    if (latestAssignment.ActualEndTime == null && latestAssignment.EntryTime <= now)
                    {
                        if (call.MaximumTime.HasValue && (call.MaximumTime.Value - now) <= riskRange)
                            return 4; // InRiskTreatment
                        return 1; // InTreatment
                    }

                    if (latestAssignment.EndType.HasValue)
                    {
                        if (latestAssignment.EndType == DO.EndType.ExpiredCancellation)
                            return 3; // Expired
                        if (latestAssignment.EndType == DO.EndType.Cared)
                            return 2; // Closed
                    }
                }

                if (call.MaximumTime.HasValue)
                {
                    if (call.MaximumTime <= now)
                        return 3; // Expired
                    if ((call.MaximumTime.Value - now) <= riskRange)
                        return 5; // OpenInRisk
                }

                return 0; // Open
            }
        }

        public static void ValidateCall(Call call)
        {
            if (call == null)
                throw new BlValidationException("Call cannot be null.");

            if (string.IsNullOrWhiteSpace(call.Address))
                throw new BlValidationException("Address cannot be null or empty.");

            if (call.OpeningTime >= call.MaximumTime)
                throw new BlValidationException("Opening time must be earlier than maximum time.");

            if (!Enum.IsDefined(typeof(CallType), call.CallType))
                throw new BlValidationException("Invalid call type.");

            if (call.VerbalDescription?.Length > 500)
                throw new BlValidationException("Verbal description is too long. Maximum length is 500 characters.");
        }

        // סינכרוני: עדכון קריאה בלי הקואורדינטות
        public static void UpdateCall(Call call)
        {
            if (call == null)
                throw new BlValidationException("Call cannot be null.");

            // יצירת DO.Call ממיפוי BO.Call, ללא שינוי קואורדינטות
            DO.Call doCall = MapToDoCall(call);

            lock (_lock)
            {
                _dal.Call.Update(doCall);
            }

            Observers.NotifyItemUpdated(doCall.Id);
            Observers.NotifyListUpdated();

            // הפעלת חישוב ועדכון הקואורדינטות אסינכרונית, לא מחכים לסיום
            _ = UpdateCoordinatesForCallAsync(doCall);
        }

        // אסינכרוני: חישוב ועדכון קואורדינטות קריאה
        private static async Task UpdateCoordinatesForCallAsync(DO.Call call)
        {
            if (!string.IsNullOrWhiteSpace(call.Address))
            {
                try
                {
                    var loc = await Tools.GetCoordinatesAsync(call.Address);

                    var updatedCall = call with { Latitude = loc.Latitude, Longitude = loc.Longitude };

                    lock (_lock)
                    {
                        _dal.Call.Update(updatedCall);
                    }

                    Observers.NotifyItemUpdated(updatedCall.Id);
                    Observers.NotifyListUpdated();
                }
                catch (Exception ex)
                {
                    // טיפול בשגיאות - ניתן להרחיב להודעה למשתמש
                    Console.WriteLine($"Failed to update coordinates for call ID={call.Id}: {ex.Message}");
                }
            }
        }

        public static double CalculateDistance(DO.Volunteer volunteer, string callAddress)
        {
            try
            {
                return volunteer.DistanceType switch
                {
                    DO.DistanceType.AirDistance => CalculateAirDistance(volunteer.Address, callAddress),
                    DO.DistanceType.WalkingDistance => CalculateWalkingDistance(volunteer.Address, callAddress),
                    DO.DistanceType.DrivingDistance => CalculateDrivingDistance(volunteer.Address, callAddress),
                    _ => throw new BlValidationException("Invalid distance type selected.")
                };
            }
            catch (Exception ex)
            {
                throw new BlException($"Failed to calculate distance for volunteer ID={volunteer.Id}. Details: {ex.Message}");
            }
        }

        public static double CalculateAirDistance(string address1, string address2)
        {
            try
            {
                var (lat1, lon1) = Tools.GetCoordinatesSync(address1);
                var (lat2, lon2) = Tools.GetCoordinatesSync(address2);

                const double R = 6371;
                double dLat = DegreesToRadians(lat2 - lat1);
                double dLon = DegreesToRadians(lon2 - lon1);
                double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                           Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                           Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                return R * c;
            }
            catch (Exception ex)
            {
                throw new BlException($"Failed to calculate air distance. Details: {ex.Message}");
            }
        }

        public static double CalculateWalkingDistance(string address1, string address2)
        {
            return GetDistanceFromOsmApi(address1, address2, "foot");
        }

        public static double CalculateDrivingDistance(string address1, string address2)
        {
            return GetDistanceFromOsmApi(address1, address2, "driving");
        }

        private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);

        private static double GetDistanceFromOsmApi(string address1, string address2, string mode)
        {
            try
            {
                var (lat1, lon1) = Tools.GetCoordinatesSync(address1);
                var (lat2, lon2) = Tools.GetCoordinatesSync(address2);

                using var client = new HttpClient();
                var url = $"https://router.project-osrm.org/route/v1/{mode}/{lon1},{lat1};{lon2},{lat2}?overview=false";
                var response = client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode)
                    throw new BlException($"Failed to retrieve distance. Status: {response.StatusCode}");

                var json = response.Content.ReadAsStringAsync().Result;
                var jsonObject = JsonDocument.Parse(json).RootElement;

                if (jsonObject.TryGetProperty("routes", out var routes) && routes.GetArrayLength() > 0)
                {
                    var distanceMeters = routes[0].GetProperty("distance").GetDouble();
                    return distanceMeters / 1000.0;
                }

                throw new BlException("No route found between the given locations.");
            }
            catch (Exception ex)
            {
                throw new BlException($"Failed to calculate {mode} distance using OpenStreetMap. Details: {ex.Message}");
            }
        }

        public static async Task SendNewCallEmailAsync(Call call)
        {
            List<DO.Volunteer> volunteers;
            lock (_lock)
            {
                volunteers = _dal.Volunteer.ReadAll(v => CalculateDistance(v, call.Address) <= v.MaxDistance).ToList();
            }
            foreach (var volunteer in volunteers)
            {
                await SendEmailAsync(volunteer.Email, "New Call Alert", $"A new call is available at {call.Address}. Please log in to accept the task.");
            }
        }

        public static async Task SendCancellationEmailAsync(string volunteerEmail, Call call)
        {
            await SendEmailAsync(volunteerEmail, "Call Assignment Canceled", $"The call at {call.Address} has been reassigned. Please check the system for updates.");
        }

        private static async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var smtpClient = new SmtpClient("sandbox.smtp.mailtrap.io")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("aad9dab9cf8dc0", "a67d754639192a"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("c0583212923@gmail.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email to {to}. Details: {ex.Message}");
            }
        }

        public static event Action? CallListUpdatedObserver;
        public static readonly Dictionary<int, Action?> SpecificCallObservers = new();

        // דוגמא למיפוי BO.Call ל-DO.Call (צריך לממש לפי המחלקות שלך)
        private static DO.Call MapToDoCall(Call call)
        {
            return new DO.Call()
            {
                Id = call.Id,
                CallType = (DO.CallType)call.CallType,
                VerbalDescription = call.VerbalDescription,
                Address = call.Address,
                Latitude = call.Latitude,
                Longitude = call.Longitude,
                OpeningTime = call.OpeningTime,
                MaximumTime = call.MaximumTime,
                // ... שדות נוספים לפי הצורך
            };
        }
        /// <summary>
        /// Updates all open calls that have expired between oldClock and newClock.
        /// </summary>
        /// <param name="oldClock">The previous clock value.</param>
        /// <param name="newClock">The updated clock value.</param>
        public static void PeriodicCallsUpdate(DateTime oldClock, DateTime newClock)
        {
            List<int> updatedAssignmentIds = new(); // לאגור מזהי משימות שעדכנו
            List<int> updatedCallIds = new();       // לאגור מזהי קריאות שצריך לעדכן למשקיפים ספציפיים

            List<DO.Call> expiredCalls;
            try
            {
                // כדי לא להחזיק lock ארוך, מבצעים קריאה לרשימה קונקרטית מחוץ לlock
                lock (_lock)
                {
                    expiredCalls = _dal.Call.ReadAll()
                        .Where(c => c.MaximumTime.HasValue && c.MaximumTime <= newClock)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to read calls during periodic update.", ex);
            }

            foreach (var call in expiredCalls)
            {
                try
                {
                    List<DO.Assignment> assignments;
                    lock (_lock)
                    {
                        assignments = _dal.Assignment.ReadAll(a => a.CallId == call.Id).ToList();
                    }

                    if (!assignments.Any())
                    {
                        var expiredAssignment = new DO.Assignment
                        {
                            CallId = call.Id,
                            VolunteerId = 0,
                            EntryTime = call.OpeningTime,
                            ActualEndTime = newClock,
                            EndType = DO.EndType.ExpiredCancellation
                        };

                        lock (_lock)
                        {
                            _dal.Assignment.Create(expiredAssignment);
                        }
                        updatedAssignmentIds.Add(expiredAssignment.Id);
                    }
                    else
                    {
                        var openAssignment = assignments.FirstOrDefault(a => a.ActualEndTime == null);
                        if (openAssignment != null)
                        {
                            var updatedAssignment = openAssignment with
                            {
                                ActualEndTime = newClock,
                                EndType = DO.EndType.ExpiredCancellation
                            };

                            lock (_lock)
                            {
                                _dal.Assignment.Update(updatedAssignment);
                            }
                            updatedAssignmentIds.Add(updatedAssignment.Id);
                        }
                    }

                    updatedCallIds.Add(call.Id);
                }
                catch (DO.DalDoesNotExistException ex)
                {
                    throw new BO.BlDoesNotExistException($"Call with ID={call.Id} does not exist.", ex);
                }
                catch (DO.DalAlreadyExistsException ex)
                {
                    throw new BO.BlDuplicateEntityException($"Assignment for call ID={call.Id} already exists.", ex);
                }
                catch (Exception ex)
                {
                    throw new BO.BlException($"An error occurred while processing call ID={call.Id}.", ex);
                }
            }

            // מחוץ ל-lock: שליחת התראות למשקיפים על כל עדכון ב assignments
            foreach (var assignmentId in updatedAssignmentIds)
            {
                Observers.NotifyItemUpdated(assignmentId);
                
            }
            Observers.NotifyListUpdated();

            // מחוץ ל-lock: שליחת התראות למשקיפים ספציפיים לקריאות
            foreach (var callId in updatedCallIds)
            {
                if (SpecificCallObservers.TryGetValue(callId, out var specificCallObserver))
                {
                    specificCallObserver?.Invoke();
                }
            }

            // מחוץ ל-lock: שליחת התראה על עדכון רשימת הקריאות
            CallListUpdatedObserver?.Invoke();
        }
      
    }
}
