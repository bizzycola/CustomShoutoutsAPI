using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;
using CustomShoutoutsAPI.DTOs;
using CustomShoutoutsAPI.Services;
using CustomShoutoutsAPI.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CustomShoutoutsAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly DataContext _ctx;
        private readonly IUserService _us;
        private readonly ITwitchService _ts;

        public AccountController(DataContext ctx, IUserService us, ITwitchService ts)
        {
            _ctx = ctx;
            _us = us;
            _ts = ts;
        }

        [HttpGet("udata")]
        [Authorize]
        public async Task<IActionResult> GetUserData()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var split = uid.Split('|');
            var twitchId = split[^1];

            var claimData = new Dictionary<string, string>();
            foreach(var claim in User.Claims)
            {
                claimData.Add(claim.Type, claim.Value);
            }

            var user = await _ts.GetUserFromId(twitchId);

            return Json(new
            {
                Success = true,
                Claims = claimData,
                UserData = user
            });
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDTO dto)
        {
            var errors = new List<string>();

            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var split = uid.Split('|');
                var userId = split[^1];

                // validate signup code
                var scode = await _ctx.SignupCodes.FirstOrDefaultAsync(p => p.Code == dto.SignupCode && !p.Used);
                if (scode == null) throw new Exception("Invalid signup code");

                scode.Used = true;
                scode.UsedAt = DateTime.UtcNow;

                var twitchData = await _ts.GetUserFromId(userId);
                if (twitchData == null)
                    throw new Exception("Twitch user profile not found");

                var valid = new CreateProfileValidator();
                var vres = valid.Validate(dto);

                if (!vres.IsValid)
                {
                    var vErrors = vres.Errors.Select(e => e.ErrorMessage).ToList();
                    errors.Add($"The following validation errors occurred: {string.Join(", ", vErrors)}");
                }

                var existUser = await _ctx.Users.FirstOrDefaultAsync(p => p.Id == userId);
                if (existUser != null)
                    errors.Add("A profile already exists for this account.");

                if (errors.Count > 0)
                    throw new Exception(string.Join(", ", errors));

                var prof = new AppUser()
                {
                    Id = userId,
                    Email = "", // Decided I probably don't need this
                    Username = twitchData.DisplayName,
                    AvatarUrl = twitchData.AvatarUrl,
                    IsAdmin = scode.IsAdmin,
                    DefaultSO = "Hey, checkout {user} at {link}! They were last seen playing {game}",
                    MaxAllowedShoutouts = 25,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                };

                _ctx.Users.Add(prof);
                await _ctx.SaveChangesAsync();

                scode.User = prof;
                await _ctx.SaveChangesAsync();

                return Json(new
                {
                    Success = true,
                    User = prof
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    Errors = errors
                });
            }
        }
    }
}
