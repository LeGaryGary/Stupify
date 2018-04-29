using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stupify.Data.CustomCommandBuiltIns;
using Stupify.Data.CustomCommandBuiltIns.HarryPotterApiCommands;
using Stupify.Data.SQL;
using Stupify.Data.SQL.Models;
using TwitchApi;

namespace Stupify.Data.Repositories
{
    internal class CustomCommandRepository : ICustomCommandRepository
    {
        private readonly BotContext _botContext;
        private readonly HarryPotterApiClient _harryPotterApiClient;
        private readonly TwitchClient _twitchClient;
        private static IEnumerable<BuiltInCommand> _builtInCommands;

        private static IServiceProvider _serviceProvider;
        public IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider != null) return _serviceProvider;

                _serviceProvider = new ServiceCollection()
                    .AddSingleton<Random>()
                    .AddTransient(sp => _harryPotterApiClient)
                    .AddTransient(sp => _twitchClient)
                    .BuildServiceProvider();

                return _serviceProvider;
            }
        }

        public CustomCommandRepository(BotContext botContext, HarryPotterApiClient harryPotterApiClient, TwitchClient twitchClient)
        {
            _botContext = botContext;
            _harryPotterApiClient = harryPotterApiClient;
            _twitchClient = twitchClient;
        }

        public async Task<string> GetCommandAsync(IGuildUser user, string commandTag)
        {
            var guildId = (long) user.GuildId;
            var command = await _botContext.CustomCommands
                .Where(c => c.ServerUser.Server.DiscordGuildId == guildId)
                .FirstOrDefaultAsync(c => c.CommandTag == commandTag).ConfigureAwait(false);

            return command?.Command;
        }

        public async Task AddCommandAsync(IGuildUser user, string commandTag, string command)
        {
            var guildId = (long) user.GuildId;
            var userId = (long) user.Id;
            var dbServerUser = await _botContext.ServerUsers
                .Where(su => su.User.DiscordUserId == userId)
                .FirstOrDefaultAsync(su => su.Server.DiscordGuildId == guildId).ConfigureAwait(false);
            _botContext.CustomCommands.Add(new CustomCommand
            {
                CommandTag = commandTag,
                Command = command,
                ServerUser = dbServerUser
            });
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<string[]> ListServerCommandsAsync(IGuildUser user)
        {
            var guildId = (long) user.GuildId;

            var commands = _botContext.CustomCommands.Where(command => command.ServerUser.Server.DiscordGuildId == guildId);

            return (await commands.ToListAsync().ConfigureAwait(false)).Select(c => c.CommandTag).ToArray();
        }

        public async Task<string[]> ListServerUserCommandsAsync(IGuildUser user)
        {
            var guildId = (long) user.GuildId;
            var userId = (long) user.Id;

            var commands = await _botContext.CustomCommands
                .Where(command => command.ServerUser.Server.DiscordGuildId == guildId)
                .Where(command => command.ServerUser.User.DiscordUserId == userId)
                .ToArrayAsync().ConfigureAwait(false);

            return commands.Select(c => c.CommandTag).ToArray();
        }

        public async Task ExecuteAsync(ICommandContext context, int argPos)
        {
            try
            {
                var commandRequest = context.Message.Content.Substring(argPos).Split(' ');

                if (commandRequest.Length == 0) return;

                var commandTag = commandRequest[0];
                var args = commandRequest.Skip(1).ToArray();
                var command = await GetCommandAsync(context.User as IGuildUser, commandTag).ConfigureAwait(false);

                if (command == null)
                {
                    await context.Channel.SendMessageAsync("You don't have access to any commands by this name").ConfigureAwait(false);
                    return;
                }

                command = ReplaceArgs(command, args);
                command = await EvaluateAsync(command).ConfigureAwait(false);

                await context.Channel.SendMessageAsync(command).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await context.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
        }

        public async Task<string> EvaluateAsync(string command)
        {
            var modifiedCommand = command;

            foreach (var builtInCommand in GetBuiltInCommands())
            {
                while (true)
                {
                    var i = modifiedCommand.IndexOf(builtInCommand.Tag, StringComparison.Ordinal);
                    if (i < 0) break;

                    var commandAfterBuiltIn = modifiedCommand.Substring(i);
                    GetArgsBoundaries(commandAfterBuiltIn, '\\', out var argStart, out var argEnd);

                    var commandArgsString = commandAfterBuiltIn.Substring(argStart + 1, argEnd - argStart - 1);
                    commandArgsString = await EvaluateAsync(commandArgsString).ConfigureAwait(false);
                    var args = SplitArgs(commandArgsString, ',', '\\');
                    //var argsTasks = args.Select(EvaluateAsync);
                    //args = await Task.WhenAll(argsTasks).ConfigureAwait(false);

                    var result = await builtInCommand.Execute(args).ConfigureAwait(false);

                    var regex = new Regex(Regex.Escape(modifiedCommand.Substring(i, argEnd + 1)));
                    modifiedCommand = regex.Replace(modifiedCommand, result, 1);

                    //modifiedCommand = modifiedCommand.Replace(modifiedCommand.Substring(i, argEnd + 1), result);
                }
            }

            return modifiedCommand;
        }

        public Task<bool> IsCreatorAsync(IGuildUser user, string commandTag)
        {
            var userId = (long) user.Id;
            return _botContext.CustomCommands
                .Where(cc => cc.ServerUser.User.DiscordUserId == userId)
                .AnyAsync(cc => cc.CommandTag == commandTag);
        }

        public async Task DeleteAsync(IGuildUser user, string commandTag)
        {
            var guildId = (long) user.GuildId;
            var command = await _botContext.CustomCommands
                .Where(cc => cc.ServerUser.Server.DiscordGuildId == guildId)
                .FirstOrDefaultAsync(cc => cc.CommandTag == commandTag).ConfigureAwait(false);

            _botContext.CustomCommands.Remove(command);

            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task EditAsync(IGuildUser user, string commandTag, string commandText)
        {
            var guildId = (long) user.GuildId;
            var command = await _botContext.CustomCommands
                .Where(cc => cc.ServerUser.Server.DiscordGuildId == guildId)
                .FirstOrDefaultAsync(cc => cc.CommandTag == commandTag).ConfigureAwait(false);

            command.Command = commandText;

            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private static string[] SplitArgs(string input, char splitter, char escape)
        {
            var inputChars = input.ToCharArray();
            var lastSplit = 0;
            var depth = 0;
            var args = new List<string>();

            for (var i = 0; i < inputChars.Length; i++)
            {
                if (inputChars[i] == splitter && (i == 0 || inputChars[i - 1] != escape) && depth == 0)
                {
                    args.Add(input.Substring(lastSplit, i - lastSplit));
                    lastSplit = i + 1;
                }
                else if (inputChars[i] == '(' && (i == 0 || inputChars[i - 1] != escape))
                {
                    depth++;
                }
                else if (inputChars[i] == ')' && (i == 0 || inputChars[i - 1] != escape))
                {
                    depth--;
                }
            }

            args.Add(input.Substring(lastSplit));

            return args.ToArray();
        }
        
        private static void GetArgsBoundaries(string input, char escape, out int start, out int end)
        {
            start = 0;
            var depth = 0;
            var inputChars = input.ToCharArray();
            for (var i = 0; i < inputChars.Length; i++)
            {
                if (inputChars[i] == '(' && (i == 0 || inputChars[i - 1] != escape))
                {
                    if (start == 0) start = i;
                    else depth++;
                }
                else if (inputChars[i] == ')')
                {
                    if (depth == 0)
                    {
                        end = i;
                        return;
                    }
                    depth--;
                }
            }

            throw new InvalidOperationException();
        }

        private IEnumerable<BuiltInCommand> GetBuiltInCommands()
        {
            if (_builtInCommands != null) return _builtInCommands;

            var commandTypes = Assembly.GetAssembly(typeof(BuiltInCommand)).GetTypes()
                .Where(type => typeof(BuiltInCommand).IsAssignableFrom(type) && type != typeof(BuiltInCommand));

            var commands = commandTypes.Select(commandType => (BuiltInCommand) commandType.GetConstructors().First().Invoke(new object[] {ServiceProvider}));
            _builtInCommands = commands;

            return _builtInCommands;
        }

        private static string ReplaceArgs(string command, string[] args)
        {
            int i;
            var replacedCommand = command;
            while ((i = replacedCommand.IndexOf("arg", StringComparison.Ordinal)) > 0)
            {
                var argNumber = int.Parse(replacedCommand.Substring(i + "arg".Length, 1));
                var arg = args[argNumber];
                replacedCommand = replacedCommand.Replace($"arg{argNumber}", arg);
            }

            return replacedCommand;
        }
    }
}
