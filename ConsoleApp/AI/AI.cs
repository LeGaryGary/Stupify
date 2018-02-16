using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.AI
{
    public class AI
    {
        private AIController _controller;

        public AI(BotContext db, Segment segment, User user)
        {
            _controller = new AIController(db, segment, user);
        }

        public async Task runAsync()
        {
            Task ai = Task.Run(() => run());
            int i = 1;
            while(!ai.IsCompleted)
            {
                await _controller.updateMsg();
                Console.WriteLine($"debug: {i++}");
                await Task.Delay(1000);
            }
        }

        public async Task run()
        {
            await Task.Delay(10000);
        }

    }
}
