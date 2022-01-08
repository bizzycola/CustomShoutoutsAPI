using CustomShoutoutsAPI.Data;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.AspNetCore.Subscriptions.Messages;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace CustomShoutoutsAPI.Auth
{
    public class SubscriptionAuthMiddleware : ISocketSessionInterceptor
    {
        public async ValueTask OnCloseAsync(ISocketConnection connection, CancellationToken cancellationToken) { }
        public async ValueTask OnRequestAsync(ISocketConnection connection, IQueryRequestBuilder requestBuilder, CancellationToken cancellationToken) { }

        public async ValueTask<ConnectionStatus> OnConnectAsync(ISocketConnection connection, InitializeConnectionMessage message, CancellationToken cancellationToken)
        {
            if(message.Payload == null)
                return ConnectionStatus.Reject("Missing message payload");

            try
            {
                using var scope = connection.HttpContext.RequestServices.CreateScope();
                var jwtHeader = (string?)message.Payload["Authorization"];
                if (jwtHeader == null || !jwtHeader.StartsWith("Bearer "))
                    return ConnectionStatus.Reject("Unauthorized");

                var token = jwtHeader.Replace("Bearer ", "");

                var opts = scope.ServiceProvider.GetRequiredService<JwtBearerOptions>();

                var claims = new JwtBearerBacker(opts).IsJwtValid(token);
                if (claims == null)
                    return ConnectionStatus.Reject("Unauthoized(invalid token)");

                var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var split = userId.Split('|');
                var twitchId = split[^1];

                connection.HttpContext.Items["userId"] = twitchId;

                // Find the user
                using var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
                var user = await ctx.Users.FirstOrDefaultAsync(p => p.Id == twitchId);
                if (user == null) return ConnectionStatus.Reject("Profile not created");

                connection.HttpContext.Items["userObj"] = user;

                return ConnectionStatus.Accept();
            }
            catch (Exception ex)
            {
                return ConnectionStatus.Reject(ex.Message);
            }
        }
    }

    public class JwtBearerBacker
    {
        public JwtBearerOptions Options { get; private set; }

        public JwtBearerBacker(JwtBearerOptions options)
        {
            this.Options = options;
        }

        public ClaimsPrincipal IsJwtValid(string token)
        {
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"https://bizzylive.us.auth0.com/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever()
            );

            var openidconfig = configManager.GetConfigurationAsync().Result;

            Options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = "https://bizzylive.us.auth0.com",
                ValidateAudience = true,
                ValidAudience = "r2uVt9N7ATg9xcyqJHy8xuomWZaIvY5i",
                ValidateLifetime = true,
                NameClaimType = ClaimTypes.NameIdentifier,

                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openidconfig.SigningKeys,
            };

            List<Exception> validationFailures = null;
            SecurityToken validatedToken;
            foreach (var validator in Options.SecurityTokenValidators)
            {
                if (validator.CanReadToken(token))
                {
                    try
                    {
                        return validator
                            .ValidateToken(token, Options.TokenValidationParameters, out validatedToken);
                    }
                    catch (Exception ex)
                    {
                        if (Options.RefreshOnIssuerKeyNotFound && Options.ConfigurationManager != null
                            && ex is SecurityTokenSignatureKeyNotFoundException)
                        {
                            Options.ConfigurationManager.RequestRefresh();
                        }

                        if (validationFailures == null)
                            validationFailures = new List<Exception>(1);

                        validationFailures.Add(ex);
                        continue;
                    }
                }
            }

            if(validationFailures != null)
            {
                foreach(var excp in validationFailures)
                {
                    Console.WriteLine("JWT Error: {0}", excp.Message);
                }
            }

            return null;
        }
    }
}
