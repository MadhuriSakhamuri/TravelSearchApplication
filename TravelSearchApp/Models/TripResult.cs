namespace TravelSearchApp.Models
{
    public class TripResult
    {
        public string Destination { get; set; }
        public Flight Flight { get; set; }
        public Hotel Hotel { get; set; }
        public int TotalCost { get; set; }
        public double Score { get; set; }
    }
}
