using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotifyApi.ResponseTypes;

namespace SpotifyApi
{
    public class SpotifyClient
    {
        public SpotifyClient(string token)
        {
            _token = token;
        }

        private readonly string _token;
        private Uri BaseUri { get; } = new Uri("https://api.spotify.com/v1/");

        private HttpClient HttpClient
        {
            get
            {
                var http = new HttpClient();
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                return http;
            }
        }

        public Task<FullTrack> GetTrackAsync(string trackId)
        {
            return GetAsync<FullTrack>($"tracks/{trackId}");
        }

        public Task<FullPlaylist> GetPlaylistAsync(string userId, string playlistId)
        {
            return GetAsync<FullPlaylist>($"users/{userId}/playlists/{playlistId}");
        }

        public Task<FullAlbum> GetAlbumAsync(string albumId)
        {
            return GetAsync<FullAlbum>($"albums/{albumId}");
        }

        private async Task<T> GetAsync<T>(string relative)
        {
            var requestUri = BaseUri.AbsoluteUri + relative;
            var response = await HttpClient.GetAsync(requestUri).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
    }
}
