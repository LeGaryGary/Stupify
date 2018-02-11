using System;
using TicTacZap;
using TicTacZap.Block;
using TicTacZap.Segment;
using TicTacZap.Segment.Blocks;

namespace TicTacZapTestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var segment = new Segment();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(segment.Render());
            segment.AddBlock(0, 0, new BasicSegmentBlock());
            segment.AddBlock(0, 4, new BasicSegmentBlock());
            segment.SetOutput(0, 0, Direction.Up);
            segment.SetOutput(0, 4, Direction.Right);
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(segment.Render());
        }
    }
}
