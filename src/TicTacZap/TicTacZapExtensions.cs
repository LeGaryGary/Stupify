using System;
using System.Collections.Generic;
using TicTacZap.Blocks;
using TicTacZap.Blocks.Production.Energy;

namespace TicTacZap
{
    public static class TicTacZapExtensions
    {
        public static decimal ResourcePerTick(this Segment segment, Resource resource)
        {
            return segment.ResourceOutput.ContainsKey(resource) ? segment.ResourceOutput[resource] : 0;
        }

        public static void SetResources(this Segment segment, Dictionary<Resource, decimal> resources)
        {
            segment.ResourceOutput = new Dictionary<Resource, decimal>();
            foreach (var resource in resources)
            {
                if (resource.Value == 0) continue;

                segment.ResourceOutput.Add(resource.Key, resource.Value);
            }
        }

        public static Dictionary<Resource, decimal> ResourcePerTick(this Segment segment)
        {
            var resources = new Dictionary<Resource, decimal>();
            foreach (Resource resource in Enum.GetValues(typeof(Resource)))
                resources.Add(resource, ResourcePerTick(segment, resource));

            return resources;
        }

        public static IBlock NewBlock(BlockType blockType)
        {
            IBlock block;

            switch (blockType)
            {
                case BlockType.Controller:
                    block = new SegmentControllerBlock();
                    break;
                case BlockType.BasicEnergy:
                    block = new BasicEnergyBlock();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(blockType), blockType, null);
            }

            return block;
        }
    }
}