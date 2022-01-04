using System.ComponentModel.DataAnnotations;

namespace CustomShoutoutsAPI.Data.Models
{
    public class TwitchAppToken
    {
        [Key]
        public string Id { get; set; } = "twitchapptoken";

        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.MinValue;
        public string TokenType { get; set; } = string.Empty;
    }
}
