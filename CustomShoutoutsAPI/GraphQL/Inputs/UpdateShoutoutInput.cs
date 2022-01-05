namespace CustomShoutoutsAPI.GraphQL.Inputs
{
    public class UpdateShoutoutInput
    {
        [GraphQLDescription("ID of the shoutout to update")]
        public Guid Id { get; set; }

        [GraphQLDescription("Templated response to return for this shoutout")]
        public string Response { get; set; } = string.Empty;
    }
}
