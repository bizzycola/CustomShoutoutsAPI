using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace CustomShoutoutsAPI.Helpers
{
    /// <summary>
    /// Authentication & Authorization Helpers
    /// </summary>
    public class AuthHelper
    {
        /// <summary>
        /// Check user is authenticated, and optionally whether their profile is created.
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="ctx">Database Context</param>
        /// <param name="doProfCheck">Whether to check the profile</param>
        /// <returns></returns>
        private static async Task AuthHandler(HttpContext context, DataContext ctx, bool doProfCheck = true)
        {
            var user = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = context.User.FindFirstValue(ClaimTypes.Email);
            if (user == null)
                throw new Exception("User not authenticated");

            context.Items.Add("userEmail", email);
            context.Items.Add("userId", user);

            var userProf = await ctx.Users.FirstOrDefaultAsync(p => p.Id == user);
            if (userProf == null && doProfCheck) //!context.Request.Path.Value.ToLower().Contains("account/create")
                throw new Exception("User profile not created");

            // if (userProf.Banned)
            //    throw new Exception($"User banned: {userProf.BanReason}");

            context.Items.Add("userObj", userProf);
        }

        /// <summary>
        /// Ensure authenticated user for standard API calls
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static async Task<bool> HandleAuth(HttpContext context, DataContext ctx)
        {
            try
            {
                await AuthHandler(context, ctx, !(context.Request.Path.Value ?? "").ToLower().Contains("account/create"));
                return true;
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { Success = false, Error = ex.Message }));
                return false;
            }
        }

        /// <summary>
        /// Ensure authenticated user for graphql queries
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static async Task HandleAuthGql(HttpContext context, DataContext ctx)
        {
            try
            {
                await AuthHandler(context, ctx, true);
            }
            catch//(Exception ex)
            {
                //context.Response.StatusCode = 401;
                //throw ex;
            }
        }

        /// <summary>
        /// Creates a <see cref="SignupCode"/> and prints it to console if there are no admin users and no admin signup codes
        /// </summary>
        /// <param name="services"></param>
        public static async void EnsureCode(IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                using (var ctx = scope.ServiceProvider.GetRequiredService<DataContext>())
                {
                    var us = scope.ServiceProvider.GetRequiredService<IUserService>();

                    var userCount = ctx.Users.Count(p => p.IsAdmin);
                    var keyCount = ctx.SignupCodes.Count(p => !p.Used && p.IsAdmin);

                    if (userCount < 1 && keyCount < 1)
                    {
                        var key = await us.CreateSignupCode("initialKey", true);
                        Console.WriteLine("Initial admin signup code: {0}", key.Code);
                    }
                }
            }
        }
    }
}
