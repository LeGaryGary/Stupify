using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Discord.Commands;

using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Commands
{
    public static class CommonFunctions
    {
        public static async Task<User> GetUserAsync(BotContext Db, SocketCommandContext Context)
        {
            return await Db.Users.FirstAsync(u => u.DiscordUserId == (long)Context.User.Id);
        }
    }
}
