namespace TravelSearchApp.Models
{
    public class Flight
    {
        public string from { get; set; }
        public string to { get; set; }
        public List<string> stops { get; set; }
        public int price { get; set; }
        public string departure_time { get; set; }
        public string arrival_time { get; set; }
    }
}
