namespace CustomShoutoutsAPI.GraphQL
{
    public class GraphQLErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (error != null && error.Exception != null)
                return error.WithMessage(error.Exception.Message);
            return error;
        }
    }
}
