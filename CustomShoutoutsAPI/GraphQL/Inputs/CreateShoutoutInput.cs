namespace CustomShoutoutsAPI.GraphQL.Inputs
{
    public class CreateShoutoutInput
    {
        [GraphQLDescription("Username of the twitch user for this shoutout")]
        public string Username { get; set; } = string.Empty;

        [GraphQLDescription("Templated response to return for this shoutout")]
        public string Response { get; set; } = string.Empty;
    }
}
