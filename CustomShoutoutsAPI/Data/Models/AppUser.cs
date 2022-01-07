using System.ComponentModel.DataAnnotations;

namespace CustomShoutoutsAPI.Data.Models
{
    public class AppUser
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }

        public string DefaultSO { get; set; } = string.Empty;

        public int MaxAllowedShoutouts { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
