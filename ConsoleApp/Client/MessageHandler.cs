using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.DataModels;
using Discord.Commands;
using Discord.WebSocket;

namespace ConsoleApp.Client
{
    internal static class MessageHandler
    {
        static MessageHandler()
        {
        }

        internal static async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;

            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(ClientManager.Client.CurrentUser, ref argPos))) return;

            // Create a Command Context
            var context = new SocketCommandContext(ClientManager.Client, message);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await ClientManager.Commands.ExecuteAsync(context, argPos);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync("Internal error whilst executing command!");
        }
    }
}