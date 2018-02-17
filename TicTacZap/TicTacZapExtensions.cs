using System;
using System.Collections.Generic;

namespace TicTacZap
{
    public static class TicTacZapExtensions
    {
        public static decimal ResourcePerTick(this Segment.Segment segment, Resource resource)
        {
            return segment.ResourceOutput.ContainsKey(resource) ? segment.ResourceOutput[resource] : 0;
        }

        public static void SetResources(this Segment.Segment segment, Dictionary<Resource, decimal> resources)
        {
            segment.ResourceOutput = new Dictionary<Resource, decimal>();
            foreach (var resource in resources)
            {
                if (resource.Value == 0) continue;
                
                segment.ResourceOutput.Add(resource.Key, resource.Value);
            }
        }

        public static Dictionary<Resource, decimal> ResourcePerTick(this Segment.Segment segment)
        {
            var resources = new  Dictionary<Resource, decimal>();
            foreach (Resource resource in Enum.GetValues(typeof(Resource)))
            {
                resources.Add(resource,ResourcePerTick(segment, resource));
            }

            return resources;
        }
    }
}
