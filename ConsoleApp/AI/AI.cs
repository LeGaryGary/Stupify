using System;
using System.Collections.Generic;
using System.Text;

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

        

    }
}
