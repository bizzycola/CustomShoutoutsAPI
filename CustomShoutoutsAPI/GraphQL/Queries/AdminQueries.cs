using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;
using CustomShoutoutsAPI.GraphQL.Inputs;
using CustomShoutoutsAPI.Helpers;
using CustomShoutoutsAPI.Services;
using Microsoft.EntityFrameworkCore;
namespace CustomShoutoutsAPI.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class AdminQueries
    {
        [GraphQLDescription("[Admin] List signup codes")]
        [UsePaging]
        public IQueryable GetSignupCodes([Service] IHttpContextAccessor http)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uobj = (AppUser?)http.HttpContext.Items["userObj"];
            if (uobj == null) throw new Exception("Not authenticated");
            if (!uobj.IsAdmin) throw new Exception("Not authorized");

            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            return ctx.SignupCodes.OrderByDescending(p => p.Created);
        }
    }
}
