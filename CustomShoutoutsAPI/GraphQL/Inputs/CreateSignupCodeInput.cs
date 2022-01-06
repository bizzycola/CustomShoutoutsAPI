namespace CustomShoutoutsAPI.GraphQL.Inputs
{
    public class CreateSignupCodeInput
    {
        public string Comment { get; set; } = string.Empty;
        public bool Admin { get; set; }
    }
}
