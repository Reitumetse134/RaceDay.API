using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Participant
    {
        public int ParticipantID { get; set; }
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public User User { get; set; }
        public ICollection<EventEnrolment> Enrolments { get; set; }
    }
}
