using System.ComponentModel.DataAnnotations;

namespace CustomShoutoutsAPI.Data.Models
{
    public class ShoutOut
    {
        [Key]
        public Guid Id { get; set; }

        public string OwnerId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;

        public string Avatar { get; set; } = string.Empty;

        public long Uses { get; set; }
        public DateTime LastCall { get;set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
