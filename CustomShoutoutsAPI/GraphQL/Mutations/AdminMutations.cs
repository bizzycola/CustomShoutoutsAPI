using CustomShoutoutsAPI.Auth;
using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;
using CustomShoutoutsAPI.GraphQL.Inputs;
using CustomShoutoutsAPI.Helpers;
using CustomShoutoutsAPI.Services;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace CustomShoutoutsAPI.GraphQL.Mutations
{
    [ExtendObjectType("Mutation")]
    public class AdminMutations
    {
        [GraphQLDescription("[Admin] Update a user")]
        [Auth]
        public async Task<bool> UpdateUserInput([Service] IHttpContextAccessor http, UpdateUserInput input)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uobj = (AppUser?)http.HttpContext.Items["userObj"];
            if (uobj == null) throw new Exception("Not authenticated");
            if (!uobj.IsAdmin) throw new Exception("Not authorized");

            if (input.MaxShoutouts < 1 || input.MaxShoutouts > 1000) throw new Exception("Shoutout count must be between 1 and 100");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            var user = await ctx.Users.FirstOrDefaultAsync(p => p.Id == input.UserId);
            if (user == null) throw new Exception("User not found");

            if (!uobj.IsSuperAdmin && user.Id == uobj.Id)
                throw new Exception("Only super administrators may modify their own accounts.");

            user.MaxAllowedShoutouts = input.MaxShoutouts;

            if (uobj.IsSuperAdmin)
            {
                if (user.IsAdmin != input.IsAdmin && user.Id == uobj.Id)
                    throw new Exception("Cannot modify own administrator state");

                user.IsAdmin = input.IsAdmin;
            }

            await ctx.SaveChangesAsync();

            return true;
        }

        [GraphQLDescription("[Admin] Send a toast notification to a user")]
        [Auth]
        public async Task<UserNotification> SendUserNotification(
            [Service] IHttpContextAccessor http,
            [Service] ITopicEventSender sender,
            SendUserNotificationInput input)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uobj = (AppUser?)http.HttpContext.Items["userObj"];
            if (uobj == null) throw new Exception("Not authenticated");
            if (!uobj.IsAdmin) throw new Exception("Not authorized");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            var toUser = await ctx.Users.FirstOrDefaultAsync(p => p.Id == input.UserId);
            if (toUser == null) throw new Exception("To user not found");

            var notifCount = await ctx.UserNotifications.CountAsync(p => p.ForId == input.UserId && !p.Read);
            if (notifCount > 3) throw new Exception("To user already has 3 pending notifications");

            var newNotif = new UserNotification()
            {
                ForId = input.UserId,
                Level = input.Type,
                Title = input.Title,
                Content = input.Message,
                Created = DateTime.UtcNow
            };

            ctx.UserNotifications.Add(newNotif);
            await ctx.SaveChangesAsync();

            await sender.SendAsync($"{input.UserId}_notifSub", newNotif);

            return newNotif;
        }

        [GraphQLDescription("[Admin] Remove an existing invite code")]
        [Auth]
        public async Task<bool> RemoveSignupCode([Service] IHttpContextAccessor http, string code)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uobj = (AppUser?)http.HttpContext.Items["userObj"];
            if (uobj == null) throw new Exception("Not authenticated");
            if (!uobj.IsAdmin) throw new Exception("Not authorized");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
            var ias = scope.ServiceProvider.GetRequiredService<IAuditService>();

            var theCode = await ctx.SignupCodes.FirstOrDefaultAsync(p => p.Code == code);
            if (theCode == null) throw new Exception("Code not found");

            ctx.SignupCodes.Remove(theCode);
            await ctx.SaveChangesAsync();

            // Audit
            try
            {
                await ias.SubmitAuditLog(uobj, AuditLogType.RemoveSignupCode, theCode.Code, $"Removed signup code");
            }
            catch { }

            return true;
        }

        [GraphQLDescription("[Admin] Create a new invite code")]
        [Auth]
        public async Task<SignupCode> CreateSignupCode([Service] IHttpContextAccessor http, CreateSignupCodeInput input)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uobj = (AppUser?)http.HttpContext.Items["userObj"];
            if (uobj == null) throw new Exception("Not authenticated");
            if (!uobj.IsAdmin) throw new Exception("Not authorized");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
            var ias = scope.ServiceProvider.GetRequiredService<IAuditService>();

            try
            {
                ctx.Attach(uobj);
            }
            catch { }

            var newCode = new SignupCode()
            {
                Code = KeyGenerator.GetUniqueKey(15),
                Used = false,
                CreatorUser = uobj,
                CreatorId = uobj.Id,
                IsAdmin = input.Admin,
                Comment = input.Comment,
                Created = DateTime.UtcNow
            };

            ctx.SignupCodes.Add(newCode);
            await ctx.SaveChangesAsync();

            // Audit
            try
            {
                await ias.SubmitAuditLog(uobj, AuditLogType.CreateSignupCode, newCode.Code, "Created signup code");
            }
            catch { }

            return newCode;
        }
    }
}
