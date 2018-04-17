using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Stupify.Data.CustomCommandBuiltIns.HarryPotterApiCommands
{
    public class HarryPotterApiClient
    {
        private readonly string _apiKey;
        private static readonly Uri BaseRoute = new Uri("https://www.potterapi.com/v1/");

        public HarryPotterApiClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        public Task<string> SortingHatAsync()
        {
            return RequestAsync<string>("sortingHat");
        }

        public async Task<Character[]> CharacterAsync(string name)
        {
            return (await RequestAsync<Character[]>($"characters?name={name}").ConfigureAwait(false));
        }

        private async Task<T> RequestAsync<T>(string route)
        {
            return JsonConvert.DeserializeObject<T>(await RequestAsync(route).ConfigureAwait(false));
        }

        private async Task<string> RequestAsync(string route)
        {
            var beforeKey = BaseRoute.AbsoluteUri + $"{route}";
            //var query = HttpUtility.ParseQueryString(beforeKey);
            var separator = beforeKey.Contains("?") ? "&" : "?";
            var httpClient = new HttpClient();
            var response = await (await httpClient.GetAsync(beforeKey + $"{separator}key={_apiKey}").ConfigureAwait(false)).Content.ReadAsStringAsync().ConfigureAwait(false);
            response = response.Replace(_apiKey, "{theSuperSecretApiKey}");
            return response;
        }
    }

    public class Character
    {
        [JsonProperty("_id")]public string Id { get; set; }
        [JsonProperty("name")]public string Name { get; set; }
        [JsonProperty("role")]public string Role { get; set; }
        [JsonProperty("house")]public string House { get; set; }
        [JsonProperty("school")]public string School { get; set; }
        [JsonProperty("__v")]public int V { get; set; }
        [JsonProperty("ministryOfMagic")]public bool MinistryOfMagic { get; set; }
        [JsonProperty("orderOfThePhoenix")]public bool OrderOfThePhoenix { get; set; }
        [JsonProperty("dumbledoresArmy")]public bool DumbledoresArmy { get; set; }
        [JsonProperty("deathEater")]public bool DeathEater { get; set; }
        [JsonProperty("bloodStatus")]public string BloodStatus { get; set; }
        [JsonProperty("species")]public string Species { get; set; }
    }
}