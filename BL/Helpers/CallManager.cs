using System.Text.Json;
using System.Web;

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
        /// API key for the LocationIQ mapping service.
        /// </summary>
        private const string LocationIqApiKey = "pk.e7a4b1005a41f28c0d56501fccf80b77";

        /// <summary>
        /// Cache for storing address coordinates to reduce API calls.
        /// </summary>
        private static readonly Dictionary<string, (double Latitude, double Longitude)> _addressCache = new();

        /// <summary>
        /// Retrieves the latitude and longitude coordinates for a given address.
        /// </summary>
        /// <param name="address">The address to fetch coordinates for.</param>
        /// <returns>A tuple containing latitude and longitude.</returns>
        private static (double Latitude, double Longitude) GetCoordinates(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new BO.BlValidationException("Address cannot be null or empty.");

            // Check if the coordinates are cached
            if (_addressCache.TryGetValue(address, out var cachedCoordinates))
                return cachedCoordinates;

            using var client = new HttpClient();
            var url = $"https://us1.locationiq.com/v1/search.php?key={LocationIqApiKey}&q={HttpUtility.UrlEncode(address)}&format=json";

            for (int i = 0; i < 3; i++) // Try up to 3 times if TooManyRequests is received
            {
                try
                {
                    var response = client.GetAsync(url).Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        if (i < 2) // If not the last attempt, wait and retry
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        throw new BO.BlException("Too many requests to the mapping service. Please try again later.");
                    }

                    if (!response.IsSuccessStatusCode)
                        throw new BO.BlException($"Failed to retrieve coordinates. Status: {response.StatusCode}");

                    var json = response.Content.ReadAsStringAsync().Result;
                    var results = JsonDocument.Parse(json).RootElement.EnumerateArray();
                    if (!results.MoveNext())
                        throw new BO.BlException("No results found for the given address.");

                    var location = results.Current;
                    double latitude = double.Parse(location.GetProperty("lat").GetString());
                    double longitude = double.Parse(location.GetProperty("lon").GetString());

                    // Store the address in the cache
                    _addressCache[address] = (latitude, longitude);
                    return (latitude, longitude);
                }
                catch (Exception ex)
                {
                    throw new BO.BlException($"Failed to retrieve coordinates for address: {address}. Details: {ex.Message}");
                }
            }

            throw new BO.BlException($"Failed to retrieve coordinates for address: {address}. Too many attempts.");
        }

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

        /// <summary>
        /// Calculates the air distance between two addresses.
        /// </summary>
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
                throw new BO.BlException($"Failed to calculate air distance. Details: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
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
    }
}
