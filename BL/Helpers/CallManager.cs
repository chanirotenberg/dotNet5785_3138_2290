using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using DalApi;


namespace Helpers
{
    internal static class CallManager
    {
        private static readonly DalApi.IDal _dal = DalApi.Factory.Get;

        /// <summary>
        /// Determines the derived status of a call based on its assignments and properties.
        /// </summary>
        /// <param name="callId">The ID of the call.</param>
        /// <returns>An integer representing the derived status of the call.</returns>
        /// 
        private const string LocationIqApiKey = "pk.e7a4b1005a41f28c0d56501fccf80b77"; // Replace with your LocationIQ API key
        private static (double Latitude, double Longitude) GetCoordinates(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or empty.");

            using var client = new HttpClient();
            var url = $"https://us1.locationiq.com/v1/search.php?key={LocationIqApiKey}&q={HttpUtility.UrlEncode(address)}&format=json";

            try
            {
                var response = client.GetAsync(url).Result; // Synchronous HTTP request
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Failed to retrieve coordinates. Status: {response.StatusCode}");

                var json = response.Content.ReadAsStringAsync().Result;
                var results = JsonDocument.Parse(json).RootElement.EnumerateArray();
                if (!results.MoveNext())
                    throw new Exception("No results found for the given address.");

                var location = results.Current;
                double latitude = double.Parse(location.GetProperty("lat").GetString());
                double longitude = double.Parse(location.GetProperty("lon").GetString());
                return (latitude, longitude);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve coordinates for address: {address}. Details: {ex.Message}");
            }
        }
        public static int DetermineCallStatus(int callId)
        {
            var now = _dal.Config.Clock; // Using system clock from config
            var riskRange = _dal.Config.RiskRange; // Using risk range from config

            // Retrieve the call from the data layer
            var call = _dal.Call.Read(callId);
            if (call == null)
                throw new ArgumentException($"Call with ID {callId} does not exist.");

            // Retrieve assignments related to the call
            var assignments = _dal.Assignment.ReadAll(a => a.CallId == callId).OrderByDescending(a => a.EntryTime).ToList();

            if (assignments.Any())
            {
                var latestAssignment = assignments.First();

                // Check if the call is currently in treatment
                if (latestAssignment.ActualEndTime == null && latestAssignment.EntryTime <= now)
                {
                    if (call.MaximumTime.HasValue && (call.MaximumTime.Value - now) <= riskRange)
                        return 4; // InRiskTreatment

                    return 1; // InTreatment
                }

                // Check if the call was closed or expired
                if (latestAssignment.EndType.HasValue)
                {
                    if (latestAssignment.EndType == DO.EndType.ExpiredCancellation)
                        return 3; // Expired

                    return 2; // Closed (handled or canceled)
                }
            }

            // Check if the call is open or in risk
            if (call.MaximumTime.HasValue)
            {
                if (call.MaximumTime <= now)
                    return 3; // Expired

                if ((call.MaximumTime.Value - now) <= riskRange)
                    return 5; // OpenInRisk
            }

            return 0; // Open
        }

        /// <summary>
        /// Validates a call object for logical and format correctness.
        /// </summary>
        /// <param name="call">The call to validate.</param>
        public static void ValidateCall(BO.Call call)
        {
            if (call == null)
                throw new ArgumentException("Call cannot be null.");

            if (string.IsNullOrWhiteSpace(call.Address))
                throw new BO.BlValidationException("Address cannot be null or empty.");

            if (call.OpeningTime >= call.MaximumTime)
                throw new BO.BlValidationException("Opening time must be earlier than maximum time.");

            // Validate call type
            if (!Enum.IsDefined(typeof(BO.CallType), call.CallType))
                throw new BO.BlValidationException("Invalid call type.");

            // Validate verbal description (optional, if applicable)
            if (call.VerbalDescription?.Length > 500)
                throw new BO.BlValidationException("Verbal description is too long. Maximum length is 500 characters.");

            if ((call.Latitude < -90 || call.Latitude > 90))
                throw new BO.BlValidationException("Latitude must be between -90 and 90 degrees.");

            if ((call.Longitude < -180 || call.Longitude > 180))
                throw new BO.BlValidationException("Longitude must be between -180 and 180 degrees.");
        

            // Additional validations can be added here
            if (!string.IsNullOrWhiteSpace(call.Address))
            {
                try
                {
                    var (latitude, longitude) = GetCoordinates(call.Address);
                    call.Latitude = latitude;
                    call.Longitude = longitude;
                }
                catch (Exception ex)
                {
                    throw new BO.BlValidationException($"Invalid address: {call.Address}. Details: {ex.Message}");
                }
            }
        }
        public static double CalculateAirDistance(string address1, string address2)
        {
            try
            {
                var (lat1, lon1) = GetCoordinates(address1);
                var (lat2, lon2) = GetCoordinates(address2);

                const double R = 6371; // Earth's radius in kilometers
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
                throw new Exception($"Failed to calculate air distance between addresses. Details: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">Degrees to convert.</param>
        /// <returns>Radians.</returns>
        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        /// <summary>
        /// Updates all open calls that have expired between oldClock and newClock.
        /// </summary>
        /// <param name="oldClock">The previous clock value.</param>
        /// <param name="newClock">The updated clock value.</param>
        public static void PeriodicCallsUpdate(DateTime oldClock, DateTime newClock)
        {
            try
            {
                // מעבר על כל הקריאות הפתוחות שפג תוקפן בזמן בין השעונים
                var expiredCalls = _dal.Call.ReadAll()
                    .Where(c => c.MaximumTime.HasValue && c.MaximumTime > oldClock && c.MaximumTime <= newClock);

                foreach (var call in expiredCalls)
                {
                    try
                    {
                        // בדיקת קריאה ללא הקצאה
                        var assignments = _dal.Assignment.ReadAll(a => a.CallId == call.Id).ToList();
                        if (!assignments.Any())
                        {
                            // יצירת הקצאה חדשה עם סוג סיום "פג תוקף"
                            var expiredAssignment = new DO.Assignment
                            {
                                CallId = call.Id,
                                VolunteerId = 0, // מתנדב לא קיים
                                EntryTime = call.OpeningTime,
                                ActualEndTime = newClock,
                                EndType = DO.EndType.ExpiredCancellation
                            };
                            _dal.Assignment.Create(expiredAssignment);
                        }
                        else
                        {
                            // עדכון הקצאה קיימת עם "פג תוקף"
                            var openAssignment = assignments.FirstOrDefault(a => a.ActualEndTime == null);
                            if (openAssignment != null)
                            {
                                var updatedAssignment = openAssignment with
                                {
                                    ActualEndTime = newClock,
                                    EndType = DO.EndType.ExpiredCancellation
                                };
                                _dal.Assignment.Update(updatedAssignment);
                            }
                        }

                        // (שלב 5) שליחת הודעה למשקיפים על הקריאה
                        if (SpecificCallObservers.TryGetValue(call.Id, out var specificCallObserver))
                        {
                            specificCallObserver?.Invoke();
                        }
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

                // (שלב 5) שליחת הודעה למשקיפים על רשימת הקריאות
                CallListUpdatedObserver?.Invoke();
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to update periodic calls.", ex);
            }
        }

        // דלגט לשליחת הודעה על עדכון רשימת הקריאות
        public static event Action? CallListUpdatedObserver;

        // מילון של משקיפים על קריאות ספציפיות
        public static readonly Dictionary<int, Action?> SpecificCallObservers = new();
    }
}
