using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Discord;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stupify.Data.Repositories;

namespace StupifyWebsite.Pages
{
    public class CustomiseGuildMessagesModel : PageModel
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomTextRepository _customTextRepository;
        private readonly IDiscordClient _client;

        public CustomiseGuildMessagesModel(IUserRepository userRepository, ICustomTextRepository customTextRepository, IDiscordClient client)
        {
            _userRepository = userRepository;
            _customTextRepository = customTextRepository;
            _client = client;
        }

        private ulong UserId => ulong.Parse(HttpContext.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);

        public Dictionary<ulong, (string Welcome, string Leaving, string Ban, string Kick)> GuildText { get; private set; }
        public Dictionary<ulong, string> GuildName { get; private set; }

        [BindProperty]public string WelcomePost { get; set; }
        [BindProperty]public string LeavingPost { get; set; }
        [BindProperty]public string BanPost { get; set; }
        [BindProperty]public string KickPost { get; set; }

        public async Task OnGetAsync()
        {
            var guilds = await _userRepository.UsersGuildsAsync(UserId).ConfigureAwait(false);

            var ownerGuilds = new List<ulong>();
            foreach (var guild in guilds)
            {
                if (await _userRepository.IsGuildOwnerAsync(UserId, guild).ConfigureAwait(false))
                {
                    ownerGuilds.Add(guild);
                }
            }
            
            GuildText = new Dictionary<ulong, (string Welcome, string Leaving, string Ban, string Kick)>();
            GuildName = new Dictionary<ulong, string>();
            foreach (var guildId in ownerGuilds)
            {
                var welcome = await _customTextRepository.GetWelcomeTextAsync(guildId).ConfigureAwait(false);
                var leaving = await _customTextRepository.GetLeaveTextAsync(guildId).ConfigureAwait(false);
                var ban = await _customTextRepository.GetBanTextAsync(guildId).ConfigureAwait(false);
                var kick = await _customTextRepository.GetKickTextAsync(guildId).ConfigureAwait(false);
                GuildText.Add(guildId, (welcome, leaving, ban, kick));

                GuildName.Add(guildId, (await _client.GetGuildAsync(guildId).ConfigureAwait(false)).Name);
            }
        }

        public async Task<IActionResult> OnPostAsync(ulong guildId)
        {
            if (!ModelState.IsValid || !await _userRepository.IsGuildOwnerAsync(UserId, guildId).ConfigureAwait(false))
            {
                return Page();
            }

            await _customTextRepository.SetWelcomeTextAsync(guildId, WelcomePost).ConfigureAwait(false);
            await _customTextRepository.SetLeaveTextAsync(guildId, LeavingPost).ConfigureAwait(false);
            await _customTextRepository.SetBanTextAsync(guildId, BanPost).ConfigureAwait(false);
            await _customTextRepository.SetKickTextAsync(guildId, KickPost).ConfigureAwait(false);
            return RedirectToPage("/CustomiseGuildMessages");
        }
    }
}