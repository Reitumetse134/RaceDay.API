using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Organiser" or "Participant"
        public string Phone { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}