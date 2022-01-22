using CustomShoutoutsAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace CustomShoutoutsAPI.Controllers
{
    [Route("api/tid")]
    [ApiController]
    public class IdController : Controller
    {
        private readonly Regex _userRegex = new("^[a-zA-Z0-9_-]*$");
        private readonly ITwitchService _ts;

        public IdController(ITwitchService ts)
        {
            _ts = ts;
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetIdFromName(string name)
        {
            try
            {
                if (name.Length < 2 || !_userRegex.IsMatch(name))
                    throw new Exception("Invalid name");

                var user = await _ts.GetUserFromName(name);
                if (user == null)
                    throw new Exception("User not found");

                return Json(new
                {
                    Success = true,
                    Id = user.Id
                });
            }
            catch(Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }
}
