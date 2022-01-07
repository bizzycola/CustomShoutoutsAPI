namespace CustomShoutoutsAPI.Data.Models
{
    public class UserNotification
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty;

        public string ForId { get; set; } = string.Empty;

        /// <summary>
        /// success, warning, error, info
        /// </summary>
        public string Level { get;set; } = String.Empty;

        public bool Read { get; set; }
        public DateTime ReadAt { get; set; }

        public DateTime Created { get; set; }
    }
}
