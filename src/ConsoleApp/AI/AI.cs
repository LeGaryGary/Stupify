using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stupify.Data.Repositories;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.AI
{
    public class AI
    {
        public enum AIStatus
        {
            Working, Finished, Stopped
        }
        private readonly AIController _controller;
        private IBlock[,] _blocks;

        public AIStatus Status { get; private set; }

        private double _breakChance;
        private decimal _considerationThreshold;
        private decimal _removeThreshold;
        private double _expansionChance;
        private double _removeChance;

        private bool _stop;
        private bool[,] _mark;
        private LinkedList<Tuple<int, int>> _placedBlocks;
        private List<Tuple<Tuple<int, int>, decimal>> _possibleExpansions;

        private Random _rnd;

        public AI(ISegmentRepository segmentRepository, int segmentId)
        {
            _controller = new AIController(segmentRepository, segmentId);
        }

        private void ResetProperties(double exp, double rmv, decimal addThr, decimal rmvThr, double brk)
        {
            _expansionChance = exp;
            _removeChance = rmv;
            _considerationThreshold = addThr;
            _removeThreshold = rmvThr;
            _breakChance = brk;
            _stop = false;

            _rnd = new Random();
            _blocks = _controller.Blocks;
            _mark = new bool[9, 9];

            // make a list of placed blocks
            _placedBlocks = new LinkedList<Tuple<int, int>>();

            for (var x = 0; x < 9; x++)
            for (var y = 0; y < 9; y++)
                if (_blocks[x, y] != null)
                {
                    _placedBlocks.AddLast(new Tuple<int, int>(x, y));
                    _mark[x, y] = true;
                }
        }

        private async Task AddBlock(Tuple<int, int> choice)
        {
            if (choice == null)
            {
                if (_rnd.NextDouble() < _breakChance)
                    return;
            }
            else
            {
                await _controller.AddBlock(choice.Item1, choice.Item2);
                _placedBlocks.AddLast(choice);
                _mark[choice.Item1, choice.Item2] = true;
                _blocks = _controller.Blocks;
            }
        }

        private async Task<decimal> Test(int x, int y, bool rmv = false)
        {
            decimal output;
            if (!rmv)
            {
                await _controller.AddBlock(x, y);
                output = _controller.Output();
                await _controller.RemoveBlock(x, y);
            }
            else
            {
                await _controller.RemoveBlock(x, y);
                output = _controller.Output();
                await _controller.AddBlock(x, y);
            }

            return output;
        }

        private Tuple<int, int> GetChoice(bool ascending = false)
        {
            if (_possibleExpansions.Count == 0) return null;
            if (!ascending)
            {
                _possibleExpansions.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            }
            else
            {
                _possibleExpansions.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            }

            var bound = _possibleExpansions.Count > 3
                ? (int) Math.Ceiling(_possibleExpansions.Count / 3d)
                : _possibleExpansions.Count;
            return _possibleExpansions[_rnd.Next(0, bound)].Item1;
        }

        public async Task Run(double exp, double rmv, decimal addThr, decimal rmvThr, double brk)
        {
            Status = AIStatus.Working;
            ResetProperties(exp, rmv, addThr, rmvThr, brk);

            var it = 0;
            while (it++ < 300 && !_stop)
            {
                _possibleExpansions = new List<Tuple<Tuple<int, int>, decimal>>();
                Tuple<int, int> choice = null;

                if (_rnd.NextDouble() < _expansionChance)
                {
                    await NewExpansion();
                    choice = GetChoice();
                    if(choice !=null)
                        await _controller.AddBlock(choice.Item1, choice.Item2);
                }
                else if (_rnd.NextDouble() < _removeChance && _placedBlocks.Count > 0)
                {
                    await RemoveBlock();
                    choice = GetChoice(true);
                    if (choice != null)
                        await _controller.RemoveBlock(choice.Item1, choice.Item2);
                }
                else
                {
                    await ExpandExisting();
                    choice = GetChoice();
                    if (choice != null)
                        await _controller.AddBlock(choice.Item1, choice.Item2);
                }


                await AddBlock(choice);
            }

            Status = (_stop) ? AIStatus.Stopped : AIStatus.Finished;
        }

        public void Stop()
        {
            _stop = true;
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

            var choice = _blocks[x, y] == null ? new Tuple<int, int>(x, y) : null;
            if (choice != null)
            {
                var output = await Test(x, y);
                _possibleExpansions.Add(new Tuple<Tuple<int, int>, decimal>(new Tuple<int, int>(x, y),
                    output));
            }
        }

        private async Task RemoveBlock()
        {
            var output = _controller.Output();
            foreach (var block in _placedBlocks)
            {
                var x = block.Item1;
                var y = block.Item2;
                var tmp = await Test(x, y, true);
                if (tmp > output || output - tmp < _removeThreshold)
                {
                    _possibleExpansions.Add(new Tuple<Tuple<int, int>, decimal>(new Tuple<int, int>(x, y),
                        tmp));
                }
            }
        }

        private async Task ExpandExisting()
        {
            var visited = (bool[,]) _mark.Clone();
            var output = _controller.Output();
            foreach (var block in _placedBlocks)
            {
                var x = block.Item1;
                var y = block.Item2;
                for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    try
                    {
                        if (!visited[x + i, y + j] && _blocks[x + i, y + j] == null)
                        {
                            visited[x + i, y + j] = true;
                            var tmp = await Test(x + i, y + j);
                            if (tmp - output > _considerationThreshold || output == 0 && tmp == output)
                                _possibleExpansions.Add(
                                    new Tuple<Tuple<int, int>, decimal>(new Tuple<int, int>(x + i, y + j),
                                        tmp));
                        }
                    }
                    catch (IndexOutOfRangeException) { }
            }
        }
    }
}