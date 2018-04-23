using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
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

        public GuildSettingsModel(ISettingsRepository settingsRepository, IUserRepository userRepository)
        {
            _settingsRepository = settingsRepository;
            _userRepository = userRepository;
        }
        
        public Dictionary<ulong, ServerSettings> Settings { get; set; }

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