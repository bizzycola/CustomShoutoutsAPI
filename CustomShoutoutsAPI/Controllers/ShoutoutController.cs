using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;
using CustomShoutoutsAPI.DTOs;
using CustomShoutoutsAPI.Services;
using CustomShoutoutsAPI.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace CustomShoutoutsAPI.Controllers
{
    [Route("api/shoutouts")]
    [ApiController]
    public class ShoutoutController : Controller
    {
        private readonly DataContext _ctx;
        private readonly IUserService _us;
        private readonly ITwitchService _ts;
        private readonly IMemoryCache _cache;

        public ShoutoutController(DataContext ctx, IUserService us, ITwitchService ts, IMemoryCache cache)
        {
            _ctx = ctx;
            _us = us;
            _ts = ts;
            _cache = cache;
        }

        [HttpGet("{owner}/{username}")]
        public async Task<IActionResult> GetShoutout(string owner, string username)
        {
            username = username.Replace("@", "").ToLower().Trim();

            var key = $"so_{owner}/{username}";
            if (!_cache.TryGetValue<string>(key, out var resp) || string.IsNullOrEmpty(resp))
            {
                var cResponse = "";

                var so = await _ctx.ShoutOuts.FirstOrDefaultAsync(p => p.OwnerId == owner && p.Username.ToLower() == username);
                if (so == null)
                {
                    var uobj = await _ctx.Users.FirstOrDefaultAsync(p => p.Id == owner);
                    if (uobj == null) return NotFound();

                    cResponse = uobj.DefaultSO;
                }
                else
                    cResponse = so.Response;

                // Get data
                var chan = await _ts.GetChannelFromName(username);
                if (chan == null) return NotFound();

                resp = cResponse;
                resp = resp.Replace("{user}", chan.ChannelName);
                resp = resp.Replace("{link}", $"https://twitch.tv/{chan.ChannelLogin}");
                resp = resp.Replace("{game}", chan.GameName);
                resp = resp.Replace("{title}", chan.Title);

                _cache.Set(key, resp, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                });
            }

            return Ok(resp);
        }
    }
}
