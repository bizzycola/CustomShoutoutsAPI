namespace CustomShoutoutsAPI.GraphQL.Inputs
{
    public class UpdateUserInput
    {
        public string UserId { get; set; } = string.Empty;
        public int MaxShoutouts { get; set; }

        public bool IsAdmin { get; set; }
    }
}
