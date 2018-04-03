using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using StupifyConsoleApp.Commands.Hotkeys;

namespace StupifyConsoleApp.Client
{
    internal class HotKeyHandler : IHotKeyHandler
    {
        private readonly ILogger<HotKeyHandler> _logger;
        static private Dictionary<char, Type> _hotKeys;

        public HotKeyHandler(ILogger<HotKeyHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ICommandContext context)
        {
            try
            {
                //Setup HotKey Cache
                if (_hotKeys == null)
                {
                    _hotKeys = new Dictionary<char, Type>();

                    var hotkeyTypes = Assembly.GetAssembly(typeof(HotKeyHandler)).GetTypes()
                        .Where(t => typeof(IHotkey).IsAssignableFrom(t))
                        .Where(t => t.IsClass);

                    foreach (var hotkeyType in hotkeyTypes)
                    {
                        var hotKey = MakeExecutor(hotkeyType);
                        _hotKeys.Add(hotKey.Key, hotkeyType);
                    }
                }

                //Get and execute hotkey
                _hotKeys.TryGetValue(context.Message.Content.ToCharArray().FirstOrDefault(), out var executorType);
                if (executorType == null) return;

                var keyExecutor = MakeExecutor(executorType);
                await keyExecutor.ExecuteAsync(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception in HotKeyHandler");
                throw;
            }
            finally
            {
                //Delete message
                await context.Message.DeleteAsync();
            }
        }

        private IHotkey MakeExecutor(Type hotKeyType)
        {
            var ctor = hotKeyType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault();
            if (ctor == null) return null;

            var paramList = new List<object>();
            foreach (var parameter in ctor.GetParameters())
            {
                paramList.Add(Config.ServiceProvider.GetService(parameter.ParameterType));
            }

            return (IHotkey)ctor.Invoke(paramList.ToArray());
        }
    }

    public interface IHotKeyHandler
    {
        Task Handle(ICommandContext message);
    }
}