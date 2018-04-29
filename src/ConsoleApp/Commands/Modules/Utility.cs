using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace StupifyConsoleApp.Commands.Modules
{
    public class Utility : ModuleBase<CommandContext>
    {
        private readonly CommandService _commandService;
        private readonly List<string> _seenModules = new List<string>();

        public Utility(CommandService commandService)
        {
            _commandService = commandService;
        }

        [Command("Help")]
        public Task HelpAsync()
        {
            var modules = _commandService.Modules
                .Where(m => m.Name != "Debug" && m.Name != "AI" && m.Name != "Solve")
                .OrderBy(m => m.Name);

            var message = "Modules:" + Environment.NewLine;
            foreach (var module in modules) message += module.Name + Environment.NewLine;
            return ReplyAsync($"```{message}```{Environment.NewLine}Use `{Config.CommandPrefix} Help [moduleName]` to find out more!{Environment.NewLine}We love feedback, Positive or negative: https://discord.gg/nb5rUhd");
        }

        [Command("Help")]
        public async Task HelpAsync(string moduleName)
        {
            var module = _commandService.Modules
                .Where(m => m.Name != "Debug" && m.Name != "AI" && m.Name != "Solve")
                .FirstOrDefault(m => string.Equals(m.Name, moduleName, StringComparison.OrdinalIgnoreCase));

            if (module == null)
            {
                await ReplyAsync(
                    $"This module doesn't exist, please use {Config.CommandPrefix}help to see a list of modules.").ConfigureAwait(false);
                return;
            }

            await ReplyAsync(RenderModuleHelp(module, string.Empty, 0)).ConfigureAwait(false);
        }

        private string RenderModuleHelp(ModuleInfo module, string prefix, int tabNumber)
        {
            var str = GetTab(tabNumber) + module.Name + ":" + Environment.NewLine;
            foreach (var command in module.Commands)
            {
                if (prefix.Contains(command.Name))
                {
                    str += GetTab(tabNumber + 1) +
                           $"-{command.Name}: `{Config.CommandPrefix}{prefix} {CommandParameterHelp(command)}`" +
                           Environment.NewLine;
                    continue;
                }

                str += GetTab(tabNumber + 1) +
                       $"-{command.Name}: `{Config.CommandPrefix}{prefix} {command.Name} {CommandParameterHelp(command)}`" +
                       Environment.NewLine;
            }

            foreach (var subModule in module.Submodules)
            {
                if (_seenModules.Contains(subModule.Name)) continue;
                _seenModules.Add(subModule.Name);
                str += RenderModuleHelp(subModule, $"{prefix} {subModule.Name}", tabNumber + 1);
            }

            return str;
        }

        private string CommandParameterHelp(CommandInfo command)
        {
            return string.Join(" ", command.Parameters.Select(p => $"[{p.Name}]"));
        }

        private string GetTab(int tabNumber)
        {
            var str = string.Empty;
            for (var i = 0; i < tabNumber; i++) str += "\t";

            return str;
        }
    }
}