using CustomShoutoutsAPI.Auth;
using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;
using CustomShoutoutsAPI.GraphQL.Inputs;
using CustomShoutoutsAPI.GraphQL.Results;
using CustomShoutoutsAPI.Helpers;
using CustomShoutoutsAPI.Services;
using Microsoft.EntityFrameworkCore;
namespace CustomShoutoutsAPI.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class AdminQueries
    {
        [GraphQLDescription("[Admin] Return site statistics")]
        [Auth]
        public async Task<SiteStatsResult> GetSiteStats([Service] IHttpContextAccessor http)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uobj = (AppUser?)http.HttpContext.Items["userObj"];
            if (uobj == null) throw new Exception("Not authenticated");
            if (!uobj.IsAdmin) throw new Exception("Not authorized");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            var res = new SiteStatsResult()
            {
                UserCount = await ctx.Users.CountAsync(),
                CustomShoutoutCount = await ctx.ShoutOuts.CountAsync(),
                TotalShoutoutCalls = await ctx.ShoutOuts.SumAsync(p => p.Uses)
            };

            return res;
        }

        [GraphQLDescription("[Admin] List signup codes")]
        [UsePaging]
        [Auth]
        public IQueryable<SignupCode> GetSignupCodes([Service] IHttpContextAccessor http)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uobj = (AppUser?)http.HttpContext.Items["userObj"];
            if (uobj == null) throw new Exception("Not authenticated");
            if (!uobj.IsAdmin) throw new Exception("Not authorized");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            return ctx.SignupCodes
                .Include(i => i.User)
                .OrderByDescending(p => p.Created);
        }

        [GraphQLDescription("[Admin] List signup codes")]
        [UsePaging]
        [Auth]
        public IQueryable<AppUser> GetUserList([Service] IHttpContextAccessor http, string? filter)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uobj = (AppUser?)http.HttpContext.Items["userObj"];
            if (uobj == null) throw new Exception("Not authenticated");
            if (!uobj.IsAdmin) throw new Exception("Not authorized");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            if (!string.IsNullOrEmpty(filter))
            {
                return ctx.Users
                    .Where(c => EF.Functions.Like(c.Username.ToUpper(), $"%{filter.ToUpper()}%"))
                    .OrderBy(p => p.Username);
            }
            else
                return ctx.Users.OrderBy(p => p.Username);
        }
    }
}
