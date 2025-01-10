using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TravelSearchApp.Models;

namespace TravelSearchApp.Controllers
{
    public class TravelController : Controller
    {
        private readonly Dictionary<string, List<Flight>> flightsByOrigin;
        private readonly Dictionary<string, List<Hotel>> hotelsByDestination;

        public TravelController()
        {
            // Load JSON data for flights and hotels
            var flightsJson = System.IO.File.ReadAllText("Flights.json");
            var hotelsJson = System.IO.File.ReadAllText("Hotels.json");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var flights = JsonSerializer.Deserialize<List<Flight>>(flightsJson, options);
            var hotels = JsonSerializer.Deserialize<List<Hotel>>(hotelsJson, options);

            // Group flights by origin for fast lookups
            flightsByOrigin = flights
                .GroupBy(f => f.from)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Group hotels by destination for fast lookups
            hotelsByDestination = hotels
                                    .GroupBy(h => ExtractCityFromAddress(h.address))
                                    .ToDictionary(g => g.Key, g => g.ToList());
        }

        private string ExtractCityFromAddress(string address)
        {
            // Split the address into parts using ',' and trim whitespace
            var parts = address.Split(',');
            if (parts.Length > 1)
            {
                return parts[1].Trim(); // Return the second part (city)
            }
            return string.Empty; // Return empty if the format is invalid
        }


        // Get Flights for a specific origin and destination
        private List<Flight> GetFlights(string origin, string destination)
        {
            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentException("Both 'origin' and 'destination' parameters are required to get flights.");
            }

            return flightsByOrigin.ContainsKey(origin)
                ? flightsByOrigin[origin].Where(f => f.to == destination).ToList()
                : new List<Flight>();
        }

        // Get Hotels for a specific destination
        private List<Hotel> GetHotels(string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentException("'destination' parameter is required to get hotels.");
            }

            return hotelsByDestination.ContainsKey(destination)
                ? hotelsByDestination[destination]
                : new List<Hotel>();
        }

        // Display the form
        public IActionResult Index()
        {
            return View();
        }

        // Handle the form submission and display results
        [HttpPost]
        public IActionResult Index(string origin, int nights, int budget)
        {
            if (string.IsNullOrWhiteSpace(origin) || nights <= 0 || budget <= 0)
            {
                ViewData["Error"] = "Please provide valid input for all fields.";
                return View();
            }

            var trips = new List<TripResult>();

            // Check if origin exists in flights
            if (!flightsByOrigin.ContainsKey(origin))
            {
                ViewData["Error"] = "No flights available from the selected origin.";
                return View();
            }

            // Iterate over destinations reachable from the origin
            foreach (var destination in flightsByOrigin[origin].Select(f => f.to).Distinct())
            {
                // Fetch all flights for this origin-destination pair
                var matchingFlights = GetFlights(origin, destination);

                // Fetch hotels for the destination
                var matchingHotels = GetHotels(destination);

                // Combine flights and hotels into trips
                foreach (var flight in matchingFlights)
                {
                    foreach (var hotel in matchingHotels)
                    {
                        var totalCost = flight.price + (hotel.price_per_night * nights);

                        if (totalCost <= budget)
                        {
                            trips.Add(new TripResult
                            {
                                Destination = destination,
                                Flight = flight,
                                Hotel = hotel,
                                TotalCost = totalCost,
                                Score = Math.Round((hotel.rating * hotel.stars) / totalCost, 2)
                            });
                        }
                    }
                }
            }

            // Sort trips by best score (descending)
            trips = trips.OrderByDescending(r => r.Score).ToList();

            ViewData["Trips"] = trips;
            return View();
        }
    }
}
