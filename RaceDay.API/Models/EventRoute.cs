using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class EventRoute  
    {
        [Key]
        public int RouteID { get; set; }
        public int EventID { get; set; }
        public string RouteName { get; set; }
        public double Distance { get; set; }
        public double ElevationGain { get; set; }
        public string Description { get; set; }
        public string MapURL { get; set; }

        public Event Event { get; set; }
    }
}