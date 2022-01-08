using CustomShoutoutsAPI.Data.Models;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;

namespace CustomShoutoutsAPI.GraphQL.Subscriptions
{
    [ExtendObjectType("Subscription")]
    public class NotificationSubscriptions
    {
        [SubscribeAndResolve]
        public ValueTask<ISourceStream<UserNotification>> OnNotification(
             [Service] ITopicEventReceiver receiver,
             [Service] IHttpContextAccessor http)
        {
            if (http.HttpContext == null)
                throw new Exception("HTTP Context not loaded");

            var uid = (string?)http.HttpContext.Items["userId"];
            if (uid == null) throw new Exception("Not authenticated");

            var topic = $"{uid}_notifSub";

            return receiver.SubscribeAsync<string, UserNotification>(topic);
        }
    }
}
