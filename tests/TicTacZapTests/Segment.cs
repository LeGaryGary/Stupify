using System.Linq;
using FluentAssertions;
using TicTacZap;
using TicTacZap.Segment.Blocks;
using TicTacZap.Segment.Blocks.Production.Energy;
using Xunit;
using TicTacZapSegment = TicTacZap.Segment.Segment;

namespace TicTacZapTests
{
    public class Segment
    {
        [Fact]
        public void AddBlockOnExistingBlock()
        {
            var segment = new TicTacZapSegment();
            segment.AddBlock(1, 1, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(1, 1, BlockType.BasicEnergy).Should().BeFalse();

            segment.ResourcePerTick(Resource.Energy).Should().Be(0);
            segment.ResourcePerTick(Resource.Unit).Should().Be(0);

            segment.Blocks[1, 1].Should().BeOfType<BasicEnergyBlock>();
            segment.Blocks[4, 4].Should().BeOfType<SegmentControllerBlock>();
        }

        [Fact]
        public void AddSingleBlock()
        {
            var segment = new TicTacZapSegment();
            segment.AddBlock(1, 1, BlockType.BasicEnergy).Should().BeTrue();

            segment.ResourcePerTick(Resource.Energy).Should().Be(0);
            segment.ResourcePerTick(Resource.Unit).Should().Be(0);

            segment.Blocks[1, 1].Should().BeOfType<BasicEnergyBlock>();
            segment.Blocks[4, 4].Should().BeOfType<SegmentControllerBlock>();
        }

        [Fact]
        public void AddTwoDiagonalNeighbourBlocks()
        {
            var segment = new TicTacZapSegment();
            segment.AddBlock(1, 1, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(2, 2, BlockType.BasicEnergy).Should().BeTrue();

            segment.ResourcePerTick(Resource.Energy).Should().Be(0);
            segment.ResourcePerTick(Resource.Unit).Should().Be(0);

            segment.Blocks[1, 1].Should().BeOfType<BasicEnergyBlock>();
            segment.Blocks[2, 2].Should().BeOfType<BasicEnergyBlock>();
            segment.Blocks[4, 4].Should().BeOfType<SegmentControllerBlock>();
        }

        [Fact]
        public void AddTwoDirectionalNeighbourBlocks()
        {
            var segment = new TicTacZapSegment();
            segment.AddBlock(1, 1, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(2, 1, BlockType.BasicEnergy).Should().BeTrue();

            segment.ResourcePerTick(Resource.Energy).Should().Be(1);
            segment.ResourcePerTick(Resource.Unit).Should().Be(0);

            segment.Blocks[1, 1].Should().BeOfType<BasicEnergyBlock>();
            segment.Blocks[2, 1].Should().BeOfType<BasicEnergyBlock>();
            segment.Blocks[4, 4].Should().BeOfType<SegmentControllerBlock>();
        }

        [Fact]
        public void AddTwoDirectionalNonNeighbourBlocks()
        {
            var segment = new TicTacZapSegment();
            segment.AddBlock(1, 1, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(3, 1, BlockType.BasicEnergy).Should().BeTrue();

            segment.ResourcePerTick(Resource.Energy).Should().Be(4);
            segment.ResourcePerTick(Resource.Unit).Should().Be(0);

            segment.Blocks[1, 1].Should().BeOfType<BasicEnergyBlock>();
            segment.Blocks[3, 1].Should().BeOfType<BasicEnergyBlock>();
            segment.Blocks[4, 4].Should().BeOfType<SegmentControllerBlock>();
        }

        [Fact]
        public void SegmentRender()
        {
            var segment = new TicTacZapSegment();
            segment.AddBlock(0, 8, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(0, 7, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(0, 4, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(0, 0, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(1, 6, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(2, 8, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(2, 1, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(4, 6, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(4, 1, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(6, 7, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(6, 1, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(7, 4, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(8, 8, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(8, 6, BlockType.BasicEnergy).Should().BeTrue();
            segment.AddBlock(8, 0, BlockType.BasicEnergy).Should().BeTrue();

            var actualText = segment.TextRender();
            var expectedText = " B  ~  B  ~  ~  ~  ~  ~  B \r\n B  ~  ~  ~  ~  ~  B  ~  ~ \r\n ~  B  ~  ~  B  ~  ~  ~  B \r\n ~  ~  ~  ~  ~  ~  ~  ~  ~ \r\n B  ~  ~  ~  C  ~  ~  B  ~ \r\n ~  ~  ~  ~  ~  ~  ~  ~  ~ \r\n ~  ~  ~  ~  ~  ~  ~  ~  ~ \r\n ~  ~  B  ~  B  ~  B  ~  ~ \r\n B  ~  ~  ~  ~  ~  ~  ~  B \r\n";
            expectedText = string.Join("",expectedText.Split().Select(t => t.Trim()).Where(t => t.Length > 0));
            actualText = string.Join("",actualText.Split().Select(t => t.Trim()).Where(t => t.Length > 0));
            actualText.Should().Be(expectedText);
        }
    }
}