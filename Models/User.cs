using System.Text.Json.Serialization;

namespace XownerWebOne.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public string Phone { get; set; }
        public string Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
