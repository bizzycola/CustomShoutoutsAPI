using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;
using CustomShoutoutsAPI.Helpers;
using Microsoft.EntityFrameworkCore;

namespace CustomShoutoutsAPI.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Creates and returns a SignupCode a new user can use to create an account
        /// </summary>
        /// <param name="creatorId">ID of the user responsible for creating the code</param>
        /// <param name="admin">Whether the user created from this code should be a site-wide admin</param>
        /// <returns></returns>
        Task<SignupCode> CreateSignupCode(string creatorId, bool admin = false);

        /// <summary>
        /// Creates and returns a SignupCode a new user can use to create an account
        /// </summary>
        /// <param name="user">User responsible for creating the code</param>
        /// <param name="admin">Whether the user created from this code should be a site-wide admin</param>
        /// <returns></returns>
        Task<SignupCode> CreateSignupCode(AppUser user, bool admin = false);

        /// <summary>
        /// Removes an existing signup code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task RemoveSignupCode(string code);

    }

    public class UserService : IUserService
    {
        private IServiceProvider _services;

        public UserService(IServiceProvider services)
        {
            _services = services;
        }

        public async Task RemoveSignupCode(string code)
        {
            using var scope = _services.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>().CreateDbContext();

            var key = await ctx.SignupCodes.FirstOrDefaultAsync(p => p.Code == code);
            if (key == null) throw new Exception("Invalid code");

            ctx.SignupCodes.Remove(key);
            await ctx.SaveChangesAsync();
        }

        public async Task<SignupCode> CreateSignupCode(AppUser user, bool admin = false)
        {
            using var scope = _services.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>().CreateDbContext();

            ctx.Attach(user);

            var code = KeyGenerator.GetUniqueKey(15);
            var signupKey = new SignupCode()
            {
                Code = code,
                Used = false,
                CreatorUser = user,
                Created = DateTime.Now,
                IsAdmin = admin
            };

            ctx.SignupCodes.Add(signupKey);
            await ctx.SaveChangesAsync();

            return signupKey;
        }

        public async Task<SignupCode> CreateSignupCode(string creatorId, bool admin = false)
        {
            using var scope = _services.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>().CreateDbContext();

            var user = await ctx.Users.FirstOrDefaultAsync(p => p.Id == creatorId);
            // if (user == null) throw new Exception("Creating user doesn't exist");

            var code = KeyGenerator.GetUniqueKey(15);
            var signupKey = new SignupCode()
            {
                Code = code,
                Used = false,
                Created = DateTime.UtcNow,
                IsAdmin = admin
            };
            if (user != null)
                signupKey.User = user;

            ctx.SignupCodes.Add(signupKey);
            await ctx.SaveChangesAsync();

            return signupKey;
        }
    }
}
