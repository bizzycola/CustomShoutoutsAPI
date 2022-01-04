using Newtonsoft.Json;

namespace CustomShoutoutsAPI.TwitchResults
{
    public class TwitchChannelDataResponse
    {
        public List<TwitchChannelData> Data { get; set; } = new List<TwitchChannelData>();
    }
    public class TwitchChannelData
    {
        [JsonProperty("broadcaster_id")]
        public string ChannelId { get; set; } = string.Empty;

        [JsonProperty("broadcaster_login")]
        public string ChannelLogin { get; set; } = string.Empty;

        [JsonProperty("broadcaster_name")]
        public string ChannelName { get; set; } = string.Empty;

        [JsonProperty("game_id")]
        public string GameId { get; set; } = string.Empty;

        [JsonProperty("game_name")]
        public string GameName { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;

    }
}
