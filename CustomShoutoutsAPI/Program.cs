using CustomShoutoutsAPI.Auth;
using CustomShoutoutsAPI.BackgroundServices;
using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.GraphQL;
using CustomShoutoutsAPI.GraphQL.Mutations;
using CustomShoutoutsAPI.GraphQL.Queries;
using CustomShoutoutsAPI.GraphQL.Subscriptions;
using CustomShoutoutsAPI.Helpers;
using CustomShoutoutsAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var services = builder.Services;
var Configuration = builder.Configuration;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

services.AddPooledDbContextFactory<DataContext>(options =>
{
    options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
});
services.AddScoped<DataContext>(p => p.GetRequiredService<IDbContextFactory<DataContext>>().CreateDbContext());

services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();

builder.Services.AddCors();


var opts = new JwtBearerOptions()
{
    Authority = "https://bizzylive.us.auth0.com",
    TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "https://bizzylive.us.auth0.com",
        ValidateAudience = true,
        ValidAudience = "r2uVt9N7ATg9xcyqJHy8xuomWZaIvY5i",
        ValidateLifetime = true,
        NameClaimType = ClaimTypes.NameIdentifier
    },

};
builder.Services.AddSingleton(opts);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = opts.Authority;
        options.TokenValidationParameters = opts.TokenValidationParameters;
    });

builder.Services.AddGraphQLServer()
                .AddAuthorization()
                .AddQueryType(d => d.Name("Query"))
                    .AddTypeExtension<UserQueries>()
                    .AddTypeExtension<ShoutoutQueries>()
                    .AddTypeExtension<AdminQueries>()
                .AddMutationType(d => d.Name("Mutation"))
                    .AddTypeExtension<ShoutoutMutations>()
                    .AddTypeExtension<UserMutations>()
                    .AddTypeExtension<AdminMutations>()
                .AddSubscriptionType(d => d.Name("Subscription"))
                    .AddTypeExtension<NotificationSubscriptions>()
                .AddInMemorySubscriptions()
                .ModifyRequestOptions(m =>
                {
                    m.IncludeExceptionDetails = true;
                })
                .AddHttpRequestInterceptor(
                    async (context, executor, builder, ct) =>
                    {
                        bool isAuthed = false;
                        try
                        {
                            var user = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                            var email = context.User.FindFirstValue(ClaimTypes.Email);
                            var split = user.Split('|');
                            var twitchId = split[^1];

                            using var scope = context.RequestServices.CreateScope();
                            using var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>().CreateDbContext();
                            
                            context.Items.Add("userId", twitchId);
                            context.Items.Add("userEmail", email);
                            context.Items.Add("userObj", await ctx.Users.FirstOrDefaultAsync(p => p.Id == twitchId));
                            isAuthed = true;
                        }
                        catch { }

                        context.Items.Add("isAuthed", isAuthed);
                    })
                .AddApolloTracing()
                .AddSocketSessionInterceptor<SubscriptionAuthMiddleware>();

services.AddErrorFilter<GraphQLErrorFilter>();

services.AddScoped<IUserService, UserService>();
services.AddHostedService<TwitchTokenService>();
services.AddScoped<ITwitchService, TwitchService>();
services.AddScoped<IAuditService, AuditService>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All,
    RequireHeaderSymmetry = false,
    ForwardLimit = null,
    KnownProxies = { IPAddress.Parse("159.203.120.225"), IPAddress.Parse("10.108.0.4") },
});

app.UseCors(
    options => options
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("Location", "Upload-Offset", "Upload-Length")
);

// app.UseHttpsRedirection();
app.UseWebSockets();

app.UseAuthentication();
app.UseAuthorization();


app.Use(async (context, next) =>
{
    // context.Request.EnableBuffering();

    using (var scope = context.RequestServices.CreateScope())
    {
        if (context.Request.Path.Value == null) return;
        if (context.Request.Path.Value.Contains("api/")
            && !context.Request.Path.StartsWithSegments("/api/account")
            && !context.Request.Path.StartsWithSegments("/api/shoutouts")
            && !context.Request.Path.StartsWithSegments("/api/tid"))
        {
            using var ctx = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>().CreateDbContext();
            if (!await AuthHelper.HandleAuth(context, ctx))
                return;
        }
    }

    await next.Invoke();
});

AuthHelper.EnsureCode(app.Services);

app.MapControllers();
app.MapGraphQL();

app.Run();
