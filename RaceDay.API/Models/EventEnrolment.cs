using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class EventEnrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }
        public int EventID { get; set; }
        public int CategoryID { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string PaymentStatus { get; set; }

        public Participant Participant { get; set; }
        public Event Event { get; set; }
        public Category Category { get; set; }
        public Result Result { get; set; }
    }
}