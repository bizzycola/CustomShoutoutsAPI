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
    public class ShoutoutMutations
    {
        private readonly Regex _userRegex = new("^[a-zA-Z0-9_-]*$");

        [GraphQLDescription("Remove a shoutout beloning to the authenticated user by ID")]
        public async Task<bool> RemoveShoutout([Service] IHttpContextAccessor http, Guid id)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uid = (string?)http.HttpContext.Items["userId"];
            if (uid == null) throw new Exception("Not authenticated");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            var existSo = await ctx.ShoutOuts.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == uid);
            if (existSo == null) throw new Exception("Shoutout not found");

            ctx.ShoutOuts.Remove(existSo);
            await ctx.SaveChangesAsync();

            return true;
        }

        [GraphQLDescription("Create a new shoutout for the authenticated user")]
        public async Task<ShoutOut> CreateShoutout([Service] IHttpContextAccessor http, CreateShoutoutInput input)
        {
            if (http.HttpContext == null) 
                throw new Exception("HTTP Context not loaded");

            var uid = (string?)http.HttpContext.Items["userId"];
            if (uid == null) throw new Exception("Not authenticated");

            if (string.IsNullOrEmpty(input.Username) || !_userRegex.IsMatch(input.Username))
                throw new Exception("Inproper twitch username");

            if (string.IsNullOrEmpty(input.Response) || input.Response.Length < 2 || input.Response.Length > 500)
                throw new Exception("Response must be between 2 and 500 characters in length.");
            
            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
            var mc = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
            var ts = scope.ServiceProvider.GetRequiredService<ITwitchService>();

            // See if we have an SO already
            var existSo = await ctx.ShoutOuts.FirstOrDefaultAsync(p => p.OwnerId == uid && p.Username.ToLower() == input.Username.ToLower().Trim());
            if (existSo != null) throw new Exception("You have already created a shoutout for this user.");

            // Check if we have mem for channel data
            var cdkey = $"chandat_{uid}_{input.Username.Trim().ToLower()}";
            if (!mc.TryGetValue<TwitchChannelData>(cdkey, out var chanData))
            {
                chanData = await ts.GetChannelFromName(input.Username.ToLower());
                if (chanData == null) throw new Exception("No channel with that name was found.");

                mc.Set(cdkey, chanData, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });
            }

            var newSo = new ShoutOut()
            {
                OwnerId = uid,
                Username = chanData.ChannelName,
                UserId = chanData.ChannelId,
                Avatar = chanData.AvatarUrl,
                Response = input.Response,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            ctx.ShoutOuts.Add(newSo);
            await ctx.SaveChangesAsync();

            return newSo;
        }
    }
}
