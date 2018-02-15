using System;
using System.Collections.Generic;
using System.Text;
using TicTacZap.Segment.Blocks;

namespace StupifyConsoleApp.TicTacZap
{
    class SerializableSegment
    {
        public List<Tuple<int, int, BlockType>> BlocksList { get; set; }
        public decimal OutputPerTick { get; set; }

        public SerializableSegment()
        {
            BlocksList = new List<Tuple<int, int, BlockType>>();
        }
    }
}
