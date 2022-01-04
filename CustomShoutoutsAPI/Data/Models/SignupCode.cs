using System.ComponentModel.DataAnnotations;

namespace CustomShoutoutsAPI.Data.Models
{
    public class SignupCode
    {
        /// <summary>
        /// The unique signup code
        /// </summary>
        [Key]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user who uses this code should be set as an admin
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// ID of the <see cref="User"/> who created this code
        /// </summary>
        public AppUser CreatorUser { get; set; } = null!;
        public string CreatorId { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the <see cref="User"/> who used this code
        /// </summary>
        public AppUser User { get; set; } = null!;

        /// <summary>
        /// Whether this code has been used
        /// </summary>
        public bool Used { get; set; }

        /// <summary>
        /// The date this code was used
        /// </summary>
        public DateTime UsedAt { get; set; }

        /// <summary>
        /// Date this code was created
        /// </summary>
        public DateTime Created { get; set; }
    }
}
