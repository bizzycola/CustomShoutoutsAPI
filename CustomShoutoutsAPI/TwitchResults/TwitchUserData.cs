using Newtonsoft.Json;

namespace CustomShoutoutsAPI.TwitchResults
{
    public class TwitchUserDataResponse
    {
        public List<TwitchUserData> Data { get; set; } = new List<TwitchUserData>();
    }

    public class TwitchUserData
    {
        public string Id { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;

        [JsonProperty("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonProperty("profile_image_url")]
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
