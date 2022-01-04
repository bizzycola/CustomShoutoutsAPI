using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.Data.Models;

namespace CustomShoutoutsAPI.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class ShoutoutQueries
    {
        [GraphQLDescription("Returns a paged list of the users custom shoutouts")]
        [UsePaging(DefaultPageSize = 100)]
        public IQueryable<ShoutOut> GetShoutouts([Service] IHttpContextAccessor http)
        {
            if (http.HttpContext == null) throw new Exception("HTTP Context not loaded");
            var scope = http.HttpContext.RequestServices.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            var uid = (string?)http.HttpContext.Items["userId"];
            if (string.IsNullOrEmpty(uid))
                throw new Exception("Not authenticated");

            return ctx
                .ShoutOuts
                .Where(p => p.OwnerId == uid)
                .OrderByDescending(p => p.Created);
        }
    }
}
