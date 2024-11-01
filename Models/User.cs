using System.ComponentModel.DataAnnotations;

namespace SampleSecureWeb.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; } // Properti ID
        public string Username { get; set; }
        public string Password { get; set; }
        public string RoleName { get; set; }
        public bool IsVerified { get; set; } // Properti IsVerified
    }
}
