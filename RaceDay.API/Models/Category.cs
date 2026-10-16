using System.ComponentModel.DataAnnotations;


namespace RaceDay.API.Models
{
    public class Category
    {

        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string CategoryName { get; set; }
        public string Gender { get; set; } // "male", "female", "mixed"
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double Distance { get; set; }
        public decimal EntryFee { get; set; }
        public Event Event { get; set; }
        public ICollection<EventEnrolment> Enrolments { get; set; }
    }
}
