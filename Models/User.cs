using System.ComponentModel.DataAnnotations;

namespace VideoStore.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Simple role storage: Admin or User
        [Required]
        public string Role { get; set; } = "User";
    }
}
