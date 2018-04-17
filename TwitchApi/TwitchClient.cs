using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TwitchApi.Responses;

namespace TwitchApi
{
    public class TwitchClient
    {
        private readonly string _clientId;

        public Uri TwitchApiUri { get; } = new Uri("https://api.twitch.tv/helix");

        private Uri UsersEndpoint => new Uri(TwitchApiUri + "/users");
        private Uri StreamsEndpoint => new Uri(TwitchApiUri + "/streams");
        private Uri GameEndpoint => new Uri(TwitchApiUri + "/games");

        public TwitchClient(string applicationClientId)
        {
            _clientId = applicationClientId;
        }

        private HttpClient HttpClient
        {
            get
            {
                var http = new HttpClient();
                http.DefaultRequestHeaders.Add("Client-ID", _clientId);
                return http;
            }
        }

        public async Task<TwitchUser> GetTwitchUserAsync(string loginName)
        {
            var response = await HttpClient.GetAsync(UsersEndpoint + $"?login={loginName}").ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TwitchResponse<TwitchUser>>(responseBody).Data.SingleOrDefault();
        }

        public async Task<bool> IsStreamingAsync(string loginName)
        {
            return await GetStreamAsync(loginName).ConfigureAwait(false) != null;
        }

        public async Task<TwitchStream> GetStreamAsync(string loginName)
        {
            var response = await HttpClient.GetAsync(StreamsEndpoint + $"?user_login={loginName}").ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TwitchResponse<TwitchStream>>(responseBody).Data.SingleOrDefault();
        }

        public async Task<TwitchGame> GetGameTitleAsync(string gameId)
        {
            var response = await HttpClient.GetAsync(GameEndpoint + $"?id={gameId}").ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TwitchResponse<TwitchGame>>(responseBody).Data.SingleOrDefault();
        }
    }
}
