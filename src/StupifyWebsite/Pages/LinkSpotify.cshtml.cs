using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stupify.Data.Models;
using Stupify.Data.Repositories;

namespace StupifyWebsite.Pages
{
    [Authorize]
    public class LinkSpotifyModel : PageModel
    {
        private readonly IExternalAccountRepository _externalAccountRepository;
        private SpotifyOptions _options;

        public LinkSpotifyModel(IOptions<SpotifyOptions> options, IExternalAccountRepository externalAccountRepository)
        {
            _externalAccountRepository = externalAccountRepository;
            _options = options.Value;
        }

        private ulong UserId => ulong.Parse(HttpContext.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);

        public async Task<IActionResult> OnGetAsync(string code)
        {
            if (await _externalAccountRepository.GetAsync(UserId, ExternalService.Spotify).ConfigureAwait(false) != null) return Page();

            if (code == null) return Redirect($"https://accounts.spotify.com/authorize?client_id={_options.AppId}&redirect_uri=http://{HttpContext.Request.Host}/LinkSpotify&response_type=code&scope=playlist-read-private%20user-library-read%20user-read-currently-playing%20user-read-recently-played%20user-top-read");

            var client = new HttpClient();
            var encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.AppId}:{_options.AppSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);

            var body = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", $"http://{HttpContext.Request.Host}/LinkSpotify")
            });
            var response = await client.PostAsync("https://accounts.spotify.com/api/token", body).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var token = JsonConvert.DeserializeObject<TokenEndpointResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            await _externalAccountRepository.AddAsync(UserId, ExternalService.Spotify,
                new ExternalAuthentication
                {
                    AccessToken = token.AccessToken,
                    TokenType = token.TokenType,
                    Scope = token.Scope,
                    ExpiresIn = token.ExpiresIn,
                    RefreshToken = token.RefreshToken
                }).ConfigureAwait(false);

            return Page();
        }
    }
}