using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stupify.Data.Encryption;
using Stupify.Data.Models;
using Stupify.Data.SQL;

namespace Stupify.Data.Repositories
{
    internal class ExternalAccountRepository : IExternalAccountRepository
    {
        private readonly BotContext _botContext;
        private readonly AesCryptography _aes;
        private readonly SpotifyOptions _options;

        public ExternalAccountRepository(BotContext botContext, AesCryptography aes, IOptions<SpotifyOptions> options)
        {
            _botContext = botContext;
            _aes = aes;
            _options = options.Value;
        }

        public async Task<ExternalAuthentication> GetAsync(ulong discordUserId, ExternalService service)
        {
            var userId = (long) discordUserId;
            var dbExternalAuth = await _botContext.ExternalAuthentication
                .Where(ea => ea.User.DiscordUserId == userId)
                .FirstOrDefaultAsync(ea => ea.Service == service).ConfigureAwait(false);

            if (dbExternalAuth == null) return null;

            var auth =  new ExternalAuthentication
            {
                AccessToken = _aes.Decrypt(dbExternalAuth.AccessTokenAes),
                TokenType = _aes.Decrypt(dbExternalAuth.TokenTypeAes),
                Scope = _aes.Decrypt(dbExternalAuth.ScopeAes),
                RefreshToken = _aes.Decrypt(dbExternalAuth.RefreshTokenAes),
                ExpiresIn = int.Parse(_aes.Decrypt(dbExternalAuth.ExpiresInAes))
            };

            if (dbExternalAuth.LastRefreshed + TimeSpan.FromSeconds(auth.ExpiresIn - 60) > DateTime.UtcNow) return auth;

            // Get New Token
            var newToken = await UpdateAsync(auth.RefreshToken, service).ConfigureAwait(false);

            // Update DB
            dbExternalAuth.AccessTokenAes = _aes.Encrypt(newToken.AccessToken);
            dbExternalAuth.ExpiresInAes = _aes.Encrypt(newToken.ExpiresIn.ToString());
            dbExternalAuth.LastRefreshed = DateTime.UtcNow;
            await _botContext.SaveChangesAsync().ConfigureAwait(false);

            // Update return object
            auth.AccessToken = newToken.AccessToken;
            auth.ExpiresIn = newToken.ExpiresIn;
            
            return auth;
        }

        private async Task<TokenEndpointResponse> UpdateAsync(string refreshToken, ExternalService service)
        {
            Uri tokenEndpoint;
            var client = new HttpClient();
            var body = new Dictionary<string, string>();
            if (service == ExternalService.Spotify)
            {
                tokenEndpoint = new Uri("https://accounts.spotify.com/api/token");
                var encodedAuth =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.AppId}:{_options.AppSecret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(service), service, null);
            }

            body.Add("grant_type", "refresh_token");
            body.Add("refresh_token", refreshToken);

            var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(body)).ConfigureAwait(false);
            var responsebody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException(responsebody);
            return JsonConvert.DeserializeObject<TokenEndpointResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public async Task AddAsync(ulong discordUserId, ExternalService service, ExternalAuthentication authentication)
        {
            if (await GetAsync(discordUserId, service).ConfigureAwait(false) != null) throw new InvalidOperationException($"The user {discordUserId} is already linked to the service {service}");
            var userId = (long) discordUserId;
            var user = await _botContext.Users.FirstOrDefaultAsync(u => u.DiscordUserId == userId).ConfigureAwait(false);
            _botContext.ExternalAuthentication.Add(new SQL.Models.ExternalAuthentication
            {
                User = user,
                Service = service,
                AccessTokenAes = _aes.Encrypt(authentication.AccessToken),
                TokenTypeAes = _aes.Encrypt(authentication.TokenType),
                ScopeAes = _aes.Encrypt(authentication.Scope),
                RefreshTokenAes = _aes.Encrypt(authentication.RefreshToken),
                ExpiresInAes = _aes.Encrypt(authentication.ExpiresIn.ToString()),
                LastRefreshed = DateTime.UtcNow
            });

            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
