using System;
using System.Collections.Generic;
using TicTacZap;
using TicTacZap.Segment.Blocks;

namespace StupifyConsoleApp.TicTacZap
{
    internal class SerializableSegment
    {
        public List<Tuple<int, int, BlockType>> BlocksList { get; set; }
        public Dictionary<Resource, decimal> Resources { get; set; }

        public SerializableSegment()
        {
            BlocksList = new List<Tuple<int, int, BlockType>>();
        }
    }
}
