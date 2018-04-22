using System;
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
        
        [BindProperty(SupportsGet = true)] public Dictionary<ulong, ServerSettings> Settings { get; set; }

        public async Task OnGetAsync()
        {
            var userId = HttpContext.User.FindFirst(c =>
                c.Type == ClaimTypes.NameIdentifier);
            var guilds = await _userRepository.UsersGuildsAsync(ulong.Parse(userId.Value)).ConfigureAwait(false);

            Settings = await _settingsRepository.GetServerSettingsAsync(guilds).ConfigureAwait(false);
        }

        public async Task<IActionResult> OnPostAsync(ulong guildId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _settingsRepository.SetServerSettingsAsync(guildId, Settings[guildId]).ConfigureAwait(false);
            return RedirectToPage("/GuildSettings");
        }
    }
}