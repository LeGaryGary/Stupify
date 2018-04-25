using Newtonsoft.Json;

namespace SpotifyApi.ResponseTypes
{
    public class CategoryList : BasicModel
    {
        [JsonProperty("categories")]
        public Paging<Category> Categories { get; set; }
    }
}