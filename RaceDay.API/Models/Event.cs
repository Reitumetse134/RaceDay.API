using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public int UserID { get; set; } // Organiser
        public string EventName { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string EventType { get; set; } // "walking", "running", "cycling"
        public User Organiser { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<EventRoute> Routes { get; set; }

    }
}
