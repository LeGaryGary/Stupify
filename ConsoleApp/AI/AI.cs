using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.AI
{
    class AI
    {
        private AIController _controller;

        public AI(BotContext db, Segment segment, User user)
        {
            _controller = new AIController(db, segment, user);
        }

        public async Task runAsync()
        {
            await Task.Run(() => run());
        }

        public void run()
        {

        }

    }
}
