using CustomShoutoutsAPI.Auth;
using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;
using CustomShoutoutsAPI.GraphQL.Inputs;
using CustomShoutoutsAPI.Helpers;
using CustomShoutoutsAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace CustomShoutoutsAPI.GraphQL.Mutations
{
    [ExtendObjectType("Mutation")]
    public class AdminMutations
    {
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
