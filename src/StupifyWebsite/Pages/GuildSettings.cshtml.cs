using System;
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

        public GuildSettingsModel(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }
        
        [BindProperty(SupportsGet = true)] public ServerSettings Settings { get; set; }

        public async Task OnGetAsync()
        {
            Settings = await _settingsRepository.GetServerSettingsAsync(0).ConfigureAwait(false);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _settingsRepository.SetServerSettingsAsync(0, Settings).ConfigureAwait(false);
            return RedirectToPage("/Index");
        }
    }
}