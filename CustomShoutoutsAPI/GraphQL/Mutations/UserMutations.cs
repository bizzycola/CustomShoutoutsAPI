using CustomShoutoutsAPI.Auth;
using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;
using CustomShoutoutsAPI.GraphQL.Inputs;
using CustomShoutoutsAPI.Services;
using CustomShoutoutsAPI.TwitchResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
namespace CustomShoutoutsAPI.GraphQL.Mutations
{
    [ExtendObjectType("Mutation")]
    public class UserMutations
    {
        [GraphQLDescription("Update the default shoutout message for the authenticated user")]
        [Auth]
        public async Task<bool> UpdateDefaultSO([Service] IHttpContextAccessor http, string so)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uid = (string?)http.HttpContext.Items["userId"];
            if (uid == null) throw new Exception("Not authenticated");

            if (string.IsNullOrEmpty(so) || so.Length < 2 || so.Length > 500)
                throw new Exception($"Shoutout must be between 2 and 500 characters in length({so}).");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            var appUser = await ctx.Users.FirstOrDefaultAsync(p => p.Id == uid);
            if (appUser == null) throw new Exception("Not authorized");

            appUser.DefaultSO = so;
            await ctx.SaveChangesAsync();

            return true;
        }
    }
}
