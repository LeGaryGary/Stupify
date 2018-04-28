using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stupify.Data.Models;
using Stupify.Data.Repositories;

namespace StupifyWebsite.Pages
{
    [Authorize]
    public class GuildSettingsModel : PageModel
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDiscordClient _client;

        public GuildSettingsModel(ISettingsRepository settingsRepository, IUserRepository userRepository, IDiscordClient client)
        {
            _settingsRepository = settingsRepository;
            _userRepository = userRepository;
            _client = client;
        }
        
        public Dictionary<ulong, ServerSettings> Settings { get; set; }
        public Dictionary<ulong, List<IRole>> Roles { get; set; }
        public Dictionary<ulong, List<ITextChannel>> TextChannels { get; set; }

        [BindProperty]public ServerSettings SettingsToSet { get; set; }

        public async Task OnGetAsync()
        {
            var guilds = await _userRepository.UsersGuildsAsync(UserId).ConfigureAwait(false);

            var ownerGuilds = new List<ulong>();
            foreach (var guild in guilds)
            {
                if (await IsGuildOwnerAsync(UserId, guild).ConfigureAwait(false))
                {
                    ownerGuilds.Add(guild);
                }
            }

            Settings = await _settingsRepository.GetServerSettingsAsync(ownerGuilds).ConfigureAwait(false);

            Roles = new Dictionary<ulong, List<IRole>>();
            TextChannels = new Dictionary<ulong, List<ITextChannel>>();
            foreach (var guildId in Settings.Keys)
            {
                var guild = await _client.GetGuildAsync(guildId).ConfigureAwait(false);
                Roles.Add(guildId, guild.Roles.ToList());
                TextChannels.Add(guildId, (await guild.GetChannelsAsync().ConfigureAwait(false)).OfType<ITextChannel>().ToList());
            }
        }

        private ulong UserId => ulong.Parse(HttpContext.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);

        public async Task<IActionResult> OnPostAsync(ulong guildId)
        {
            if (!ModelState.IsValid || !await IsGuildOwnerAsync(UserId, guildId).ConfigureAwait(false))
            {
                return Page();
            }

            await _settingsRepository.SetServerSettingsAsync(guildId, SettingsToSet).ConfigureAwait(false);
            return RedirectToPage("/GuildSettings");
        }

        private Task<bool> IsGuildOwnerAsync(ulong userId, ulong guildId)
        {
            return _userRepository.IsGuildOwnerAsync(userId, guildId);
        }
    }
}