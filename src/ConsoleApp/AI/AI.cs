using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.AI
{
    public class AI
    {
        private readonly AIController _controller;

        public AI(BotContext db, Segment segment, User user)
        {
            _controller = new AIController(db, segment, user);
        }

        public async Task Run()
        {
            var rnd = new Random();
            var blocks = _controller.Blocks;

            // make a list of placed blocks
            var placedBlocks = new LinkedList<Tuple<int, int>>();

            for(var x = 0; x < 9; x++)
                for(var y = 0; y < 9; y++)
                {
                    if(blocks[x, y] != null)
                    {
                        placedBlocks.AddLast(new Tuple<int, int>(x, y));
                    }
                }

            var it = 0;
            while (it++ < 200)
            {
                // iterate through the list, choosing the best block to expand
                var output = _controller.Output();
                Tuple<int, int> best = null;
                foreach(var block in placedBlocks)
                {
                    var x = block.Item1;
                    var y = block.Item2;
                    for(var i = -1; i <= 1; i++)
                        for(var j = -1; j <= 1; j++)
                        { 
                            if(x+i >= 0 && y+j >=0 && x+i < 9 && y+j < 9 
                                && blocks[x+i, y+j] == null)
                            {
                                await _controller.AddBlock(x + i, y + j);
                                decimal tmp = _controller.Output();
                                if(tmp - output > 50 || (output == 0 && tmp == output))
                                {
                                    output = tmp;
                                    best = new Tuple<int, int>(x + i, y + j);
                                }
                                await _controller.RemoveBlock(x + i, y + j);
                            }
                        }
                }

                if (best == null) break;
                // add the block to list
                await _controller.AddBlock(best.Item1, best.Item2);
                placedBlocks.AddLast(best);
                blocks = _controller.Blocks;
                await _controller.UpdateDb();
            }
            
        }

    }
}
