using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyApi.ResponseTypes
{
    public class AvailabeDevices : BasicModel
    {
        [JsonProperty("devices")]
        public List<Device> Devices { get; set; }
    }
}