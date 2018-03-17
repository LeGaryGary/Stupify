using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TicTacZap;
using TicTacZap.Blocks;

namespace StupifyConsoleApp.TicTacZapManagement
{
    internal static class Segments
    {
        private const string SegmentExtension = ".SEG";
        private static readonly string SegmentsPath;
        private static readonly List<Tuple<int, Segment>> SegmentCache;

        private static readonly Random Random;

        static Segments()
        {
            SegmentsPath = Config.DataDirectory + @"\Segments";
            SegmentCache = new List<Tuple<int, Segment>>();
            Random = new Random();

            Directory.CreateDirectory(SegmentsPath);
        }

        public static async Task<Segment> GetAsync(int segmentId)
        {
            var segmentTuple = SegmentCache.FirstOrDefault(s => s.Item1 == segmentId);
            if (segmentTuple != null) return segmentTuple.Item2;
            var segment = await LoadSegmentFileAsync(segmentId);
            if (SegmentCache.Count >= 1000) SegmentCache.RemoveAt(Random.Next(SegmentCache.Count));
            SegmentCache.Add(new Tuple<int, Segment>(segmentId, segment));
            return segment;
        }

        public static async Task SetAsync(int segmentId, Segment segment)
        {
            var segmentTuple = SegmentCache.FirstOrDefault(s => s.Item1 == segmentId);
            if (segmentTuple != null)
            {
                SegmentCache.Remove(segmentTuple);
                SegmentCache.Add(new Tuple<int, Segment>(segmentId, segment));
            }

            await SaveSegmentFileAsync(segmentId, segment);
        }

        public static async Task NewSegmentAsync(int segmentId)
        {
            var segment = new Segment();
            if (SegmentCache.Count >= 1000) SegmentCache.RemoveAt(Random.Next(SegmentCache.Count));
            SegmentCache.Add(new Tuple<int, Segment>(segmentId, segment));
            await SaveSegmentFileAsync(segmentId, segment);
        }

        private static async Task<Segment> LoadSegmentFileAsync(int segmentId)
        {
            var fileText = await File.ReadAllTextAsync(SegmentsPath + $@"\{segmentId}{SegmentExtension}");
            return DeserializeSegment(fileText);
        }

        private static async Task SaveSegmentFileAsync(int segmentId, Segment segment)
        {
            var fileText = SerializeSegment(segment);

            var streamWriter = File.CreateText(SegmentsPath + $@"\{segmentId + SegmentExtension}");
            await streamWriter.WriteAsync(fileText);
            streamWriter.Close();
        }

        public static async Task DeleteSegmentAsync(int segmentId)
        {
            await Task.Run(() => DeleteSegment(segmentId));
        }

        private static void DeleteSegment(int segmentId)
        {
            var segmentTuple = SegmentCache.FirstOrDefault(s => s.Item1 == segmentId);
            SegmentCache.Remove(segmentTuple);
            File.Delete(SegmentsPath + "\\" + segmentId + SegmentExtension);
        }

        public static async Task<Dictionary<BlockType, int>> ResetSegmentAsync(int segmentId)
        {
            var blocks = new Dictionary<BlockType, int>();
            var segment = await GetAsync(segmentId);

            for (var x = 0; x < 9; x++)
            for (var y = 0; y < 9; y++)
            {
                var blockType = segment.DeleteBlock(x, y);
                if (blockType == null) continue;
                if (blocks.ContainsKey(blockType.Value))
                    blocks[blockType.Value]++;
                else
                    blocks.Add(blockType.Value, 1);
            }

            await SaveSegmentFileAsync(segmentId, segment);
            return blocks;
        }

        public static async Task<bool> AddBlockAsync(int segmentId, int x, int y, BlockType blockType)
        {
            var segment = await GetAsync(segmentId);
            var addBlockResult = segment.AddBlock(x, y, blockType);
            if (addBlockResult) await SetAsync(segmentId, segment);
            return addBlockResult;
        }

        public static async Task<BlockType?> DeleteBlockAsync(int segmentId, int x, int y)
        {
            var segment = await GetAsync(segmentId);
            var deleteBlockResult = segment.DeleteBlock(x, y);
            if (deleteBlockResult != null) await SaveSegmentFileAsync(segmentId, segment);
            return deleteBlockResult;
        }

        public static async Task<Dictionary<Resource, decimal>> GetOutput(int segmentId)
        {
            return (await GetAsync(segmentId)).ResourcePerTick();
        }

        public static string SerializeSegment(Segment segment)
        {
            var serSeg = new SerializableSegment();

            var blocks = segment.Blocks;
            for (var y = 0; y < blocks.GetLength(1); y++)
            for (var x = 0; x < blocks.GetLength(0); x++)
            {
                var block = blocks[x, y];
                if (block == null) continue;

                serSeg.BlocksList.Add(new Tuple<int, int, BlockType, int>(x, y, block.BlockType, block.Health));
            }

            serSeg.Resources = segment.ResourcePerTick();

            return JsonConvert.SerializeObject(serSeg);
        }

        public static Segment DeserializeSegment(string fileText)
        {
            var deserialized = JsonConvert.DeserializeObject<SerializableSegment>(fileText);
            var segment = new Segment();
            var blocks = segment.Blocks;
            foreach (var tuple in deserialized.BlocksList)
            {
                blocks[tuple.Item1, tuple.Item2] = TicTacZapExtensions.NewBlock(tuple.Item3, tuple.Item1, tuple.Item2);
                blocks[tuple.Item1, tuple.Item2].Health = tuple.Item4;
            }

            segment.SetResources(deserialized.Resources);
            return segment;
        }
    }
}