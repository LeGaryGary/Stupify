using System;
using System.Collections.Generic;
using TicTacZap;
using TicTacZap.Blocks;

namespace Stupify.Data.FileSystem
{
    internal class SerializableSegment
    {
        public SerializableSegment()
        {
            BlocksList = new List<Tuple<int, int, BlockType, int>>();
        }

        public List<Tuple<int, int, BlockType, int>> BlocksList { get; set; }
        public Dictionary<Resource, decimal> Resources { get; set; }
    }
}