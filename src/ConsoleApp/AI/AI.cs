using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.AI
{
    public class AI
    {
        private const double ExpansionChance = 0.1;
        private const decimal ConsiderationThreshold = 5;
        private const double BreakChance = 0.05;

        private readonly AIController _controller;

        public AI(BotContext db, Segment segment, User user)
        {
            _controller = new AIController(db, segment, user);
        }

        public async Task Run()
        {
            var rnd = new Random();
            var blocks = _controller.Blocks;
            var mark = new bool[9, 9];

            // make a list of placed blocks
            var placedBlocks = new LinkedList<Tuple<int, int>>();

            for(var x = 0; x < 9; x++)
                for(var y = 0; y < 9; y++)
                {
                    if(blocks[x, y] != null)
                    {
                        placedBlocks.AddLast(new Tuple<int, int>(x, y));
                        mark[x, y] = true;
                    }
                }

            var it = 0;
            while (it++ < 200)
            {
                // iterate through the list, choosing the best block to expand
                Tuple<int, int> choice = null;

                if (rnd.NextDouble() < ExpansionChance)
                {
                    var x = -1;
                    var y = -1;
                    var i = 0;
                    do
                    {
                        x = rnd.Next(0, 9);
                        y = rnd.Next(0, 9);
                    } while (blocks[x, y] != null && i++ < 20);
                    choice = (blocks[x, y] == null) ? new Tuple<int, int>(x, y) : null;
                }
                else
                {
                    var possibleExpansions = new List<Tuple<Tuple<int, int>, decimal>>();
                    var visited = (bool[,])mark.Clone();
                    var output = _controller.Output();
                    foreach (var block in placedBlocks)
                    {
                        var x = block.Item1;
                        var y = block.Item2;
                        for (var i = -1; i <= 1; i++)
                        for (var j = -1; j <= 1; j++)
                        {
                            if (x + i >= 0 && y + j >= 0 && x + i < 9 && y + j < 9
                                && !visited[x + i, y + j] && blocks[x + i, y + j] == null)
                            {
                                visited[x + i, y + j] = true;
                                await _controller.AddBlock(x + i, y + j);
                                var tmp = _controller.Output();
                                if (tmp - output > ConsiderationThreshold || (output == 0 && tmp == output))
                                {
                                    possibleExpansions.Add(
                                        new Tuple<Tuple<int, int>, decimal>(new Tuple<int, int>(x + i, y + j),
                                            tmp));
                                }
                                await _controller.RemoveBlock(x + i, y + j);
                            }
                        }
                        await ClientManager.LogAsync($"{possibleExpansions.Count}");
                        possibleExpansions.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                        var bound = (possibleExpansions.Count > 3) ? (int)Math.Ceiling(possibleExpansions.Count / 3d) : possibleExpansions.Count;
                        choice = possibleExpansions[rnd.Next(0, bound)].Item1;
                    }
                }

                if (choice == null)
                    if (rnd.NextDouble() < BreakChance)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                // add the block to list
                await _controller.AddBlock(choice.Item1, choice.Item2);
                placedBlocks.AddLast(choice);
                mark[choice.Item1, choice.Item2] = true;
                blocks = _controller.Blocks;
                await _controller.UpdateDb();
            }
            
        }

    }
}
