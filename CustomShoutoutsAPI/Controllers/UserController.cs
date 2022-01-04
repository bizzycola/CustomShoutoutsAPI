using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;
using CustomShoutoutsAPI.DTOs;
using CustomShoutoutsAPI.Services;
using CustomShoutoutsAPI.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomShoutoutsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var user = (AppUser?)HttpContext.Items["userObj"];
            if (user == null) return Json(new { Success = false });

            return Json(new
            {
                Success = true,
                Me = user
            });
        }
    }
}
