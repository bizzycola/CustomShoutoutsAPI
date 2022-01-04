using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;
using CustomShoutoutsAPI.GraphQL.Inputs;
using CustomShoutoutsAPI.Services;
using CustomShoutoutsAPI.TwitchResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
namespace CustomShoutoutsAPI.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class UserQueries
    {
        [GraphQLDescription("Return the default shoutout set on the authenticated account")]
        public async Task<string> GetDefaultSO([Service] IHttpContextAccessor http)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uid = (string?)http.HttpContext.Items["userId"];
            if (uid == null) throw new Exception("Not authenticated");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            var appUser = await ctx.Users.FirstOrDefaultAsync(p => p.Id == uid);
            if (appUser == null) throw new Exception("Not authorized");

            return appUser.DefaultSO;
        }
    }
}
