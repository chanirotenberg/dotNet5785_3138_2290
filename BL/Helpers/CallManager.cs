using System.Net.Mail;
using System.Net;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Helpers
{
    /// <summary>
    /// Manages call-related operations, including status determination and validation.
    /// </summary>
    internal static class CallManager
    {
        /// <summary>
        /// Data access layer instance.
        /// </summary>
        private static readonly DalApi.IDal _dal = DalApi.Factory.Get;

        /// <summary>
        /// Determines the status of a call based on its assignments and properties.
        /// </summary>
        /// <param name="callId">The ID of the call.</param>
        /// <returns>An integer representing the call status.</returns>
        public static int DetermineCallStatus(int callId)
        {
            var now = _dal.Config.Clock;
            var riskRange = _dal.Config.RiskRange;

            var call = _dal.Call.Read(callId)
                ?? throw new BO.BlDoesNotExistException($"Call with ID {callId} does not exist.");

            var assignments = _dal.Assignment.ReadAll(a => a.CallId == callId)
                .OrderByDescending(a => a.EntryTime)
                .ToList();

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

                    return 2; // Closed (handled or canceled)
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

        /// <summary>
        /// Validates a call object for logical and format correctness.
        /// </summary>
        /// <param name="call">The call to validate.</param>
        public static void ValidateCall(BO.Call call)
        {
            if (call == null)
                throw new BO.BlValidationException("Call cannot be null.");

            if (string.IsNullOrWhiteSpace(call.Address))
                throw new BO.BlValidationException("Address cannot be null or empty.");

            if (call.OpeningTime >= call.MaximumTime)
                throw new BO.BlValidationException("Opening time must be earlier than maximum time.");

            if (!Enum.IsDefined(typeof(BO.CallType), call.CallType))
                throw new BO.BlValidationException("Invalid call type.");

            if (call.VerbalDescription?.Length > 500)
                throw new BO.BlValidationException("Verbal description is too long. Maximum length is 500 characters.");

            if ((call.Latitude < -90 || call.Latitude > 90))
                throw new BO.BlValidationException("Latitude must be between -90 and 90 degrees.");

            if ((call.Longitude < -180 || call.Longitude > 180))
                throw new BO.BlValidationException("Longitude must be between -180 and 180 degrees.");

            if (!string.IsNullOrWhiteSpace(call.Address))
            {
                try
                {
                    var (latitude, longitude) = Tools.GetCoordinates(call.Address);
                    call.Latitude = latitude;
                    call.Longitude = longitude;
                }
                catch (Exception ex)
                {
                    throw new BO.BlValidationException($"Invalid address: {call.Address}. Details: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Calculates the distance based on the volunteer's chosen distance type.
        /// </summary>
        /// <param name="volunteer">The volunteer for whom the distance is calculated.</param>
        /// <param name="callAddress">The address of the call.</param>
        /// <returns>The calculated distance in kilometers.</returns>
        public static double CalculateDistance(DO.Volunteer volunteer, string callAddress)
        {
            try
            {
                return volunteer.DistanceType switch
                {
                    DO.DistanceType.AirDistance => CalculateAirDistance(volunteer.Address, callAddress),
                    DO.DistanceType.WalkingDistance => CalculateWalkingDistance(volunteer.Address, callAddress),
                    DO.DistanceType.DrivingDistance => CalculateDrivingDistance(volunteer.Address, callAddress),
                    _ => throw new BO.BlValidationException("Invalid distance type selected.")
                };
            }
            catch (Exception ex)
            {
                throw new BO.BlException($"Failed to calculate distance for volunteer ID={volunteer.Id}. Details: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the air distance between two addresses.
        /// </summary>
        public static double CalculateAirDistance(string address1, string address2)
        {
            try
            {
                var (lat1, lon1) = Tools.GetCoordinates(address1);
                var (lat2, lon2) = Tools.GetCoordinates(address2);

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
                throw new BO.BlException($"Failed to calculate air distance. Details: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the walking distance between two addresses.
        /// </summary>
        public static double CalculateWalkingDistance(string address1, string address2)
        {
            return GetDistanceFromOsmApi(address1, address2, "foot");
        }

        /// <summary>
        /// Calculates the driving distance between two addresses.
        /// </summary>
        public static double CalculateDrivingDistance(string address1, string address2)
        {
            return GetDistanceFromOsmApi(address1, address2, "driving");
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        /// <summary>
        /// Retrieves the distance between two addresses using OpenStreetMap API.
        /// </summary>
        private static double GetDistanceFromOsmApi(string address1, string address2, string mode)
        {
            try
            {
                var (lat1, lon1) = Tools.GetCoordinates(address1);
                var (lat2, lon2) = Tools.GetCoordinates(address2);

                using var client = new HttpClient();
                var url = $"https://router.project-osrm.org/route/v1/{mode}/{lon1},{lat1};{lon2},{lat2}?overview=false";

                var response = client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode)
                    throw new BO.BlException($"Failed to retrieve distance. Status: {response.StatusCode}");

                var json = response.Content.ReadAsStringAsync().Result;
                var jsonObject = JsonDocument.Parse(json).RootElement;

                if (jsonObject.TryGetProperty("routes", out var routes) && routes.GetArrayLength() > 0)
                {
                    var distanceMeters = routes[0].GetProperty("distance").GetDouble();
                    return distanceMeters / 1000.0; // Convert meters to kilometers
                }

                throw new BO.BlException("No route found between the given locations.");
            }
            catch (Exception ex)
            {
                throw new BO.BlException($"Failed to calculate {mode} distance using OpenStreetMap. Details: {ex.Message}");
            }
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
                // Retrieve all calls that have expired between oldClock and newClock
                var expiredCalls = _dal.Call.ReadAll()
                    .Where(c => c.MaximumTime.HasValue && c.MaximumTime > oldClock && c.MaximumTime <= newClock);

                foreach (var call in expiredCalls)
                {
                    try
                    {
                        // Check if the call has any existing assignments
                        var assignments = _dal.Assignment.ReadAll(a => a.CallId == call.Id).ToList();
                        if (!assignments.Any())
                        {
                            // Create a new assignment for the expired call with "Expired Cancellation" status
                            var expiredAssignment = new DO.Assignment
                            {
                                CallId = call.Id,
                                VolunteerId = 0, // No volunteer assigned
                                EntryTime = call.OpeningTime,
                                ActualEndTime = newClock,
                                EndType = DO.EndType.ExpiredCancellation
                            };
                            _dal.Assignment.Create(expiredAssignment);
                        }
                        else
                        {
                            // Update an existing assignment for the expired call
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

                        // Notify observers of the specific call update (if applicable)
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

                // Notify observers that the call list has been updated
                CallListUpdatedObserver?.Invoke();
            }
            catch (Exception ex)
            {
                throw new BO.BlException("Failed to update periodic calls.", ex);
            }
        }


        /// <summary>
        /// Event to notify observers about call list updates.
        /// </summary>
        public static event Action? CallListUpdatedObserver;

        /// <summary>
        /// Dictionary storing observers for specific calls.
        /// </summary>
        public static readonly Dictionary<int, Action?> SpecificCallObservers = new();


        /// <summary>
        /// Sends an email notification to volunteers about a new call.
        /// </summary>
        /// <param name="call">The call details.</param>
        public static void SendNewCallEmail(BO.Call call)
        {
            var volunteers = _dal.Volunteer.ReadAll(v => CalculateDistance(v, call.Address) <= v.MaxDistance);
            foreach (var volunteer in volunteers)
            {
                SendEmail(volunteer.Email, "New Call Alert", $"A new call is available at {call.Address}. Please log in to accept the task.");
            }
        }

        /// <summary>
        /// Sends an email notification to a volunteer when an assignment is canceled.
        /// </summary>
        /// <param name="volunteerEmail">The volunteer's email address.</param>
        /// <param name="call">The call details.</param>
        public static void SendCancellationEmail(string volunteerEmail, BO.Call call)
        {
            SendEmail(volunteerEmail, "Call Assignment Canceled", $"The call at {call.Address} has been reassigned. Please check the system for updates.");
        }

        /// <summary>
        /// Sends an email using SMTP.
        /// </summary>
        /// <param name="to">Recipient email.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email body.</param>
        private static void SendEmail(string to, string subject, string body)
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

                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email to {to}. Details: {ex.Message}");
            }
        }

    }
}
