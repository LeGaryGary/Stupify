using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Stupify.Data.Repositories;

namespace StupifyConsoleApp.Commands.Modules
{
    public class CustomCommand : ModuleBase<CommandContext>
    {
        private readonly ICustomCommandRepository _commandRepository;

        public CustomCommand(ICustomCommandRepository commandRepository)
        {
            _commandRepository = commandRepository;
        }

        [Command("AddCommand")]
        public async Task AddCommand(string commandTag, [Remainder] string command)
        {
            var existingCommand = await _commandRepository.GetCommandAsync(Context.User as IGuildUser, commandTag).ConfigureAwait(false);
            if (existingCommand == null)
            {
                await _commandRepository.AddCommandAsync(Context.User as IGuildUser, commandTag, command).ConfigureAwait(false);
                await ReplyAsync($"{commandTag} has been added!").ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync($"{commandTag} already exists!").ConfigureAwait(false);
            }
        }

        [Command("MyCommands")]
        public async Task MyCommands()
        {
            var commands = await _commandRepository.ListServerCommandsAsync(Context.User as IGuildUser).ConfigureAwait(false);

            if (commands.Length == 0)
            {
                await ReplyAsync($"You have no commands! use `{Config.CommandPrefix} AddCommand` to create a new command!").ConfigureAwait(false);
                return;
            }

            var message = new StringBuilder();

            foreach (var command in commands)
            {
                message.Append(command + Environment.NewLine);
            }

            await ReplyAsync(message.ToString()).ConfigureAwait(false);
        }
    }
}
