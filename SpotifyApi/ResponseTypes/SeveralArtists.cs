using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyApi.ResponseTypes
{
    public class SeveralArtists : BasicModel
    {
        [JsonProperty("artists")]
        public List<FullArtist> Artists { get; set; }
    }
}