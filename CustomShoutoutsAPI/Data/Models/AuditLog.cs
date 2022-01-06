namespace CustomShoutoutsAPI.Data.Models
{
    public enum AuditLogType
    {
        CreateSignupCode,
        RemoveSignupCode
    }

    /// <summary>
    /// Admin audit logs
    /// </summary>
    public class AuditLog
    {
        public Guid Id { get; set; }

        public AppUser AuditedUser { get; set; } = null!;

        /// <summary>
        /// ID of the affected object
        /// </summary>
        public string ObjectId { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public DateTime Created { get; set; }
    }
}
