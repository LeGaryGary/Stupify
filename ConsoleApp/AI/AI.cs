using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using StupifyConsoleApp.DataModels;
using TicTacZap.Segment.Blocks;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp.AI
{
    public class AI
    {
        private AIController _controller;

        public AI(BotContext db, Segment segment, User user)
        {
            _controller = new AIController(db, segment, user);
        }

        public async Task run()
        {
            Random rnd = new Random();
            IBlock[,] blocks = _controller.Blocks;

            // make a list of placed blocks
            LinkedList<Tuple<int, int>> placedBlocks = new LinkedList<Tuple<int, int>>();

            for(int x = 0; x < 9; x++)
                for(int y = 0; y < 9; y++)
                {
                    if(blocks[x, y] != null)
                    {
                        placedBlocks.AddLast(new Tuple<int, int>(x, y));
                    }
                }

            int it = 0;
            while (it++ < 200)
            {
                // iterate through the list, choosing the best block to expand
                decimal output = _controller.output();
                Tuple<int, int> best = null;
                foreach(Tuple<int, int> block in placedBlocks)
                {
                    int x = block.Item1;
                    int y = block.Item2;
                    for(int i = -1; i <= 1; i++)
                        for(int j = -1; j <= 1; j++)
                        { 
                            if(x+i >= 0 && y+j >=0 && x+i < 9 && y+j < 9 
                                && blocks[x+i, y+j] == null)
                            {
                                await _controller.addBlock(x + i, y + j);
                                decimal tmp = _controller.output();
                                if(tmp - output > 50 || (output == 0 && tmp == output))
                                {
                                    output = tmp;
                                    best = new Tuple<int, int>(x + i, y + j);
                                }
                                await _controller.removeBlock(x + i, y + j);
                            }
                        }
                }

                if (best == null) break;
                // add the block to list
                await _controller.addBlock(best.Item1, best.Item2);
                placedBlocks.AddLast(best);
                blocks = _controller.Blocks;
                await _controller.updateDB();
            }
            
        }

    }
}
