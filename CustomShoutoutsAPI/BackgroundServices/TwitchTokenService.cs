using CustomShoutoutsAPI.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;

namespace CustomShoutoutsAPI.BackgroundServices
{
    public class TwitchTokenService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly RestClient _client;

        public TwitchTokenService(IServiceProvider services, IConfiguration config)
        {
            _services = services;
            _configuration = config;

            _client = new RestClient("https://id.twitch.tv/oauth2");
        }

        private async Task<RefreshedToken?> GetAppToken()
        {
            try
            {
                var clientId = _configuration["Twitch:ClientId"];
                var secret = _configuration["Twitch:ClientSecret"];
                if (clientId == null || secret == null) return null;

                var request = new RestRequest("token", DataFormat.Json);
                request.AddParameter("client_id", clientId);
                request.AddParameter("client_secret", secret);
                request.AddParameter("grant_type", "client_credentials");

                return await _client.PostAsync<RefreshedToken>(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Refresh App Token] Error: {0}", ex.Message);
                return null;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    using var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

                    var appTok = await ctx.TwitchAppToken.FirstOrDefaultAsync(stoppingToken);
                    if (appTok == null)
                    {
                        var newTok = await GetAppToken();
                        if (newTok == null) continue;

                        appTok = new Data.Models.TwitchAppToken()
                        {
                            AccessToken = newTok.AccessToken,
                            ExpiresIn = newTok.ExpiresIn,
                            RefreshToken = newTok.RefreshToken,
                            TokenType = newTok.TokenType,
                            ExpiresAt = DateTime.UtcNow.AddSeconds(newTok.ExpiresIn)
                        };
                        ctx.TwitchAppToken.Add(appTok);
                        await ctx.SaveChangesAsync(stoppingToken);
                    }
                    else
                    {
                        if (DateTime.UtcNow.AddHours(2) > appTok.ExpiresAt)
                        {
                            var newTok = await GetAppToken();
                            if (newTok == null) continue;

                            appTok.ExpiresIn = newTok.ExpiresIn;
                            appTok.RefreshToken = newTok.RefreshToken;
                            appTok.AccessToken = newTok.AccessToken;
                            appTok.TokenType = newTok.TokenType;
                            appTok.ExpiresAt = DateTime.UtcNow.AddSeconds(appTok.ExpiresIn);

                            await ctx.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Refresh App Token Service] Error: {0}", ex.Message);
                }

                await Task.Delay(3600_000, stoppingToken);
            }
        }
    }

    public class RefreshedToken
    {
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
