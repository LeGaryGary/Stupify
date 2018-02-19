using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using StupifyConsoleApp.Client;
using StupifyConsoleApp.DataModels;
using TicTacZap.Segment.Blocks;

namespace StupifyConsoleApp.AI
{
    public class AI
    {
        private double _expansionChance;
        private decimal _considerationThreshold;
        private double _breakChance;

        private Random _rnd;
        private IBlock[,] _blocks;
        private bool[,] _mark;
        private LinkedList<Tuple<int, int>> _placedBlocks;
        private List<Tuple<Tuple<int, int>, decimal>> _possibleExpansions;

        private readonly AIController _controller;

        public AI(BotContext db, Segment segment, User user)
        {
            _controller = new AIController(db, segment, user);
        }

        private void ResetProperties(double exp, decimal thr, double brk)
        {
            _expansionChance = exp;
            _considerationThreshold = thr;
            _breakChance = brk;

            _rnd = new Random();
            _blocks = _controller.Blocks;
            _mark = new bool[9, 9];

            // make a list of placed blocks
            _placedBlocks = new LinkedList<Tuple<int, int>>();

            for (var x = 0; x < 9; x++)
            for (var y = 0; y < 9; y++)
            {
                if (_blocks[x, y] != null)
                {
                    _placedBlocks.AddLast(new Tuple<int, int>(x, y));
                    _mark[x, y] = true;
                }
            }
        }

        private async Task AddBlock(Tuple<int, int> choice)
        {
            if (choice == null)
                if (_rnd.NextDouble() < _breakChance)
                    return;
                else
                {
                    await _controller.AddBlock(choice.Item1, choice.Item2);
                    _placedBlocks.AddLast(choice);
                    _mark[choice.Item1, choice.Item2] = true;
                    _blocks = _controller.Blocks;
                    await _controller.UpdateDb();
                }
        }

        private async Task<decimal> Test(int x, int y)
        {
            await _controller.AddBlock(x, y);
            var output = _controller.Output();
            await _controller.RemoveBlock(x, y);

            return output;
        }

        private Tuple<int, int> GetChoice()
        {
            if (_possibleExpansions.Count == 0) return null;
            _possibleExpansions.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            var bound = (_possibleExpansions.Count > 3) ? (int)Math.Ceiling(_possibleExpansions.Count / 3d) : _possibleExpansions.Count;
            return _possibleExpansions[_rnd.Next(0, bound)].Item1;
        }

        public async Task Run(double exp, decimal thr, double brk)
        {
            ResetProperties(exp, thr, brk);

            var it = 0;
            while (it++ < 200)
            {
                _possibleExpansions = new List<Tuple<Tuple<int, int>, decimal>>();

                if (_rnd.NextDouble() < _expansionChance)
                {
                    await NewExpansion();
                }
                else
                {
                    await ExpandExisting();
                }

                var choice = GetChoice();
                await AddBlock(choice);
            }
        }

        private async Task NewExpansion()
        {
            var x = -1;
            var y = -1;
            var i = 0;
            do
            {
                x = _rnd.Next(0, 9);
                y = _rnd.Next(0, 9);
            } while (_blocks[x, y] != null && i++ < 20);
            var choice = (_blocks[x, y] == null) ? new Tuple<int, int>(x, y) : null;
            if (choice != null)
            {
                var output = await Test(x, y);
                _possibleExpansions.Add(new Tuple<Tuple<int, int>, decimal>(new Tuple<int, int>(x, y),
                    output));
            }
        }

        private async Task ExpandExisting()
        {
            var visited = (bool[,])_mark.Clone();
            var output = _controller.Output();
            foreach (var block in _placedBlocks)
            {
                var x = block.Item1;
                var y = block.Item2;
                for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                {
                    try
                    {
                        if (!visited[x + i, y + j] && _blocks[x + i, y + j] == null)
                        {
                            visited[x + i, y + j] = true;
                            var tmp = await Test(x + i, y + j);
                            if (tmp - output > _considerationThreshold || (output == 0 && tmp == output))
                            {
                                _possibleExpansions.Add(
                                    new Tuple<Tuple<int, int>, decimal>(new Tuple<int, int>(x + i, y + j),
                                        tmp));
                            }
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        continue;
                    }
                }
            }
        }


    }
}
