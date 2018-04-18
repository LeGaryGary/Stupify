using Newtonsoft.Json;

namespace TwitchApi.Responses
{
    public class TwitchUser
    {
        [JsonProperty("broadcaster_type")] public string BroadcasterType { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("display_name")] public string DisplayName { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("login")] public string LoginName { get; set; }
        [JsonProperty("offline_image_url")] public string OfflineImageUrl { get; set; }
        [JsonProperty("profile_image_url")] public string ProfileImageUrl { get; set; }
        [JsonProperty("type")] public string UserType { get; set; }
        [JsonProperty("view_count")] public int ViewCount { get; set; }
    }
}