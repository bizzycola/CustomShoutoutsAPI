using CustomShoutoutsAPI.Data;
using CustomShoutoutsAPI.TwitchResults;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace CustomShoutoutsAPI.Services
{
    public interface ITwitchService
    {
        Task<TwitchChannelData?> GetChannelFromName(string channelName);
        Task<TwitchChannelData?> GetChannelFromId(string channelId);
        Task<TwitchUserData?> GetUserFromName(string name);
        Task<TwitchUserData?> GetUserFromId(string id);
    }

    public class TwitchService : ITwitchService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly RestClient _client;

        public TwitchService(IServiceProvider services, IConfiguration config)
        {
            _configuration = config;
            _services = services;
            _client = new RestClient("https://api.twitch.tv/helix");
            _client.UseNewtonsoftJson();
        }

        public async Task<TwitchChannelData?> GetChannelFromName(string channelName)
        {
            var udata = await GetUserFromName(channelName);
            if (udata == null) throw new Exception("User not found");

            var chanData = await GetChannelFromId(udata.Id);
            if (chanData == null) throw new Exception("Channel not found");

            chanData.AvatarUrl = udata.AvatarUrl;

            return chanData;
        }

        public async Task<TwitchChannelData?> GetChannelFromId(string channelId)
        {
            using var scope = _services.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            // find app token
            var clientId = _configuration["Twitch:ClientId"];
            var appToken = await ctx.TwitchAppToken.FirstAsync();
            if (appToken == null) throw new Exception("App token missing");

            var request = new RestRequest("channels", DataFormat.Json);
            request.AddHeader("Authorization", $"Bearer {appToken.AccessToken}");
            request.AddHeader("Client-Id", clientId);
            request.AddQueryParameter("broadcaster_id", channelId);

            var resp = await _client.GetAsync<TwitchChannelDataResponse>(request);
            return resp.Data?.FirstOrDefault();
        }

        public async Task<TwitchUserData?> GetUserFromName(string name)
        {
            using var scope = _services.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            // find app token
            var clientId = _configuration["Twitch:ClientId"];
            var appToken = await ctx.TwitchAppToken.FirstAsync();
            if (appToken == null) throw new Exception("App token missing");

            var request = new RestRequest("users", DataFormat.Json);
            request.AddHeader("Authorization", $"Bearer {appToken.AccessToken}");
            request.AddHeader("Client-Id", clientId);
            request.AddQueryParameter("login", name);


            var resp = await _client.GetAsync<TwitchUserDataResponse>(request);
            return resp.Data.FirstOrDefault();
        }

        public async Task<TwitchUserData?> GetUserFromId(string id)
        {
            using var scope = _services.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();

            // find app token
            var clientId = _configuration["Twitch:ClientId"];
            var appToken = await ctx.TwitchAppToken.FirstAsync();
            if (appToken == null) throw new Exception("App token missing");

            var request = new RestRequest("users", DataFormat.Json);
            request.AddHeader("Authorization", $"Bearer {appToken.AccessToken}");
            request.AddHeader("Client-Id", clientId);
            request.AddQueryParameter("id", id);


            var resp = await _client.GetAsync<TwitchUserDataResponse>(request);
            return resp.Data.FirstOrDefault();
        }
    }

    
}
