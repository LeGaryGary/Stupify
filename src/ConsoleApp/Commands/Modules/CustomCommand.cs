using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Stupify.Data.Repositories;
using StupifyConsoleApp.Commands.Conditions;

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
        public async Task AddCommandAsync(string commandTag, [Remainder] string command)
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

        [Command("CustomCommands")]
        public async Task CustomCommandsAsync()
        {
            var commands = await _commandRepository.ListServerCommandsAsync(Context.User as IGuildUser).ConfigureAwait(false);

            if (commands.Length == 0)
            {
                await ReplyAsync($"There are no commands! use `{Config.CommandPrefix} AddCommand` to create a new command!").ConfigureAwait(false);
                return;
            }

            var message = new StringBuilder();

            foreach (var command in commands)
            {
                message.Append(command + Environment.NewLine);
            }

            await ReplyAsync(message.ToString()).ConfigureAwait(false);
        }

        [Command("MyCustomCommands")]
        public async Task MyCustomCommandsAsync()
        {
            var commands = await _commandRepository.ListServerUserCommandsAsync(Context.User as IGuildUser).ConfigureAwait(false);

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

        [Command("Evaluate")]
        public async Task EvaluateAsync([Remainder] string command)
        {
            try
            {
                await ReplyAsync(await _commandRepository.EvaluateAsync(command).ConfigureAwait(false)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message).ConfigureAwait(false);
            }
        }

        
        [Command("ForceDeleteCommand")]
        [Moderator]
        public async Task ForceDeleteCommandAsync(string commandTag)
        {
            await _commandRepository.DeleteAsync(Context.User as IGuildUser, commandTag).ConfigureAwait(false);
            await ReplyAsync("GONE...OBLITERATED...ERASED...*forever...*").ConfigureAwait(false);
        }

        [Command("DeleteCommand")]
        public async Task DeleteCommandAsync(string commandTag)
        {
            if (await _commandRepository.IsCreatorAsync(Context.User as IGuildUser, commandTag).ConfigureAwait(false))
            {
                await _commandRepository.DeleteAsync(Context.User as IGuildUser, commandTag).ConfigureAwait(false);
                await ReplyAsync("GONE...OBLITERATED...ERASED...*forever...*").ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("You um... don't own this command, so no?").ConfigureAwait(false);
            }
        }

        [Command("EditCommand")]
        public async Task EditCommandAsync(string commandTag, [Remainder]string commandText)
        {
            if (await _commandRepository.IsCreatorAsync(Context.User as IGuildUser, commandTag).ConfigureAwait(false))
            {
                await _commandRepository.EditAsync(Context.User as IGuildUser, commandTag, commandText).ConfigureAwait(false);
                await ReplyAsync("There we go, good as new!").ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("You um... don't own this command, so no?").ConfigureAwait(false);
            }
        }
    }
}
