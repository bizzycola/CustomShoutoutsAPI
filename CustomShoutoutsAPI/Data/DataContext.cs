using CustomShoutoutsAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomShoutoutsAPI.Data
{
    public class DataContext : DbContext
    {
        public DbSet<AppUser> Users { get; set; }
        public DbSet<SignupCode> SignupCodes { get; set; }
        public DbSet<TwitchAppToken> TwitchAppToken { get; set; }
        public DbSet<ShoutOut> ShoutOuts { get; set; }

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }
    }
}
