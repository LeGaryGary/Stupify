using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using StupifyConsoleApp.TicTacZapManagement;

namespace StupifyConsoleApp.Commands.Modules.TicTacZap
{
    public class Universe : StupifyModuleBase
    {
        [Command("Universe")]
        public async Task ShowUniverseCommand()
        {
            await ReplyAsync(UniverseController.RenderTheEntiretyOfCreationAsWeKnowIt());
        }
    }
}
