namespace TravelSearchApp.Models
{
    public class Hotel
    {
        public string name { get; set; }
        public string address { get; set; }
        public int stars { get; set; }
        public double rating { get; set; }
        public List<string> amenities { get; set; }
        public int price_per_night { get; set; }
    }
}
