using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;

namespace CustomShoutoutsAPI.Services
{
    public interface IAuditService
    {
        Task SubmitAuditLog(AppUser trigger, AuditLogType type, string objectId, string comment);
    }

    public class AuditService : IAuditService
    {
        private readonly DataContext _ctx;

        public AuditService(DataContext ctx)
        {
            _ctx = ctx;
        }

        public async Task SubmitAuditLog(AppUser trigger, AuditLogType type, string objectId, string comment)
        {
            try
            {
                _ctx.Attach(trigger);
            }
            catch { }

            var auditLog = new AuditLog()
            {
                AuditedUser = trigger,
                Comment = comment,
                ObjectId = objectId,
                Created = DateTime.UtcNow
            };

            _ctx.AuditLogs.Add(auditLog);
            await _ctx.SaveChangesAsync();
        }
    }
}
