using System;
using System.Threading.Tasks;
using Discord.Commands;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp.Commands
{
    public class UtilityModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            var commands = ClientManager.Commands.Commands;
            var message = string.Empty;
            foreach (var command in commands)
            {
                if(command.Module.Name == "DebugModule") continue;

                message += $"{command.Name}: `{Config.CommandPrefix}{command.Name} ";
                var parameters = command.Parameters;
                foreach (var parameter in parameters)
                {
                    message += $"[{parameter.Name}] ";
                }

                message += "`"+Environment.NewLine;
            }

            await ReplyAsync(message);
        }
    }
}
