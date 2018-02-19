using System;
using FluentAssertions;
using TicTacZap.Segment.Blocks.Production.Energy;
using Xunit;

namespace TicTacZapTests
{
    public class Block
    {
        [Fact]
        public void ResourceFormulae()
        {
            var block = new BasicEnergyBlock();
            block.UpdateOutput(4738910, 0).Should().Be(4738910);
        }

        [Fact]
        public void ResourceFormulaeSingleNeighbour()
        {
            var block = new BasicEnergyBlock();
            block.UpdateOutput(4738910, 1).Should().Be(2369455);
        }
    }
}
