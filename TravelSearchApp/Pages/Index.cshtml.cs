using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TravelSearchApp.Models;

public class IndexModel : PageModel
{
    public List<TripResult> Trips { get; set; } = new List<TripResult>();

    [BindProperty]
    public string Origin { get; set; } // Form input for Origin

    [BindProperty]
    public int Nights { get; set; } // Form input for Nights

    [BindProperty]
    public int Budget { get; set; } // Form input for Budget

    private readonly List<Flight> flights;
    private readonly List<Hotel> hotels;

    public IndexModel()
    {
        // Load flights and hotels data from JSON files
        var flightsJson = System.IO.File.ReadAllText("Flights.json");
        var hotelsJson = System.IO.File.ReadAllText("Hotels.json");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        flights = JsonSerializer.Deserialize<List<Flight>>(flightsJson, options);
        hotels = JsonSerializer.Deserialize<List<Hotel>>(hotelsJson, options);
    }

    public void OnGet()
    {
        Console.WriteLine("Flights loaded: " + flights.Count);
        Console.WriteLine("Hotels loaded: " + hotels.Count);
        // Default GET request logic (optional)
    }

    public void OnPost()
    {
        // Process form submission and search for trips
        Trips = new List<TripResult>();

        foreach (var flight in flights.Where(f => f.from == Origin))
        {
            var destinationHotels = hotels.Where(h => h.address.Contains(flight.to));

            foreach (var hotel in destinationHotels)
            {
                var totalCost = flight.price + (hotel.price_per_night * Nights);

                if (totalCost <= Budget)
                {
                    Trips.Add(new TripResult
                    {
                        Destination = flight.to,
                        Flight = flight,
                        Hotel = hotel,
                        TotalCost = totalCost,
                        Score = Math.Round((hotel.rating * hotel.stars) / totalCost, 2)
                    });
                }
            }
        }

        Trips = Trips.OrderByDescending(r => r.Score).ToList();
    }
}
