using CustomShoutoutsAPI.Data.Models;
using HotChocolate.Resolvers;
using HotChocolate.Types.Descriptors;
using System.Reflection;

namespace CustomShoutoutsAPI.Auth
{
    public class AuthDirective
    {
        public bool RequireAdmin { get; set; }
        public AuthDirective(bool requireAdmin = false)
        {
            RequireAdmin = requireAdmin;
        }
    }

    public class AuthMiddleware
    {
        private readonly FieldDelegate _next;

        public AuthMiddleware(FieldDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(IDirectiveContext context) //IDirectiveContext
        {
            try
            {
                AuthDirective directive = context.Directive
                    .ToObject<AuthDirective>();

                IServiceProvider services = context.Service<IServiceProvider>();
                using (var scope = services.CreateScope())
                {
                    var http = services.GetRequiredService<IHttpContextAccessor>();
                    if (http.HttpContext == null) throw new Exception("Null HTTPContext");

                    var userId = (string?)http.HttpContext.Items["userId"];
                    var user = http.HttpContext.Items["userObj"];

                    if (string.IsNullOrEmpty(userId))
                    {
                        SetError(context, directive, AuthState.NoUser);
                        return;
                    }

                    if (user == null || user is not AppUser)
                    {
                        SetError(context, directive, AuthState.NoProfile);
                        return;
                    }

                    if (directive.RequireAdmin && !((AppUser)user).IsAdmin)
                    {
                        SetError(context, directive, AuthState.NoAdmin);
                        return;
                    }


                    await _next(context).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private bool IsErrorResult(IDirectiveContext context) =>
            context.Result is IError || context.Result is IEnumerable<IError>;

        private void SetError(
            IDirectiveContext context,
            AuthDirective directive,
            AuthState state)
        {

            var error = "";

            if (state == AuthState.NoUser)
                error = "User not authenticated";
            else if (state == AuthState.NoProfile)
                error = "User profile not created";
            else if (state == AuthState.NoAdmin)
                error = "User not authorized for this resource";
            else if (state == AuthState.InvalidToken)
                error = "Github access token invalidated";

            context.Result = ErrorBuilder.New()
                        .SetMessage(error)
                        .SetCode(ErrorCodes.Authentication.NotAuthenticated)
                        .SetPath(context.Path)
                        .AddLocation(context.Selection.SyntaxNode)
                        .Build();
        }

        private enum AuthState
        {
            NoUser,
            NoProfile,
            NoAdmin,
            InvalidToken
        }
    }

    public class AuthDirectiveType : DirectiveType<AuthDirective>
    {
        protected override void Configure(IDirectiveTypeDescriptor<AuthDirective> desc)
        {
            desc
                .Name("auth")
                .Location(DirectiveLocation.Schema)
                .Location(DirectiveLocation.Object)
                .Location(DirectiveLocation.FieldDefinition)
                .Repeatable();

            desc.Use<AuthMiddleware>();
        }
    }

    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Property
        | AttributeTargets.Method,
        Inherited = true,
        AllowMultiple = true)]
    public class AuthAttribute
    : ObjectFieldDescriptorAttribute
    {
        public bool RequireAdmin { get; set; }
        public AuthAttribute(bool requireAdmin)
        {
            RequireAdmin = requireAdmin;
        }

        public AuthAttribute() { RequireAdmin = false; }

        public override void OnConfigure(
            IDescriptorContext context,
            IObjectFieldDescriptor descriptor,
            MemberInfo member)
        {
            //descriptor.Use<AuthDirective>();
            descriptor.Directive(CreateDirective());
        }

        private AuthDirective CreateDirective()
        {
            return new AuthDirective(RequireAdmin);
        }
    }
}
