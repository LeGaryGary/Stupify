using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TicTacZap;
using TicTacZap.Segment;
using TicTacZap.Segment.Blocks;

namespace StupifyConsoleApp.TicTacZap
{
    internal static class Segments
    {
        private static readonly string SegmentsPath;
        private const string SegmentExtension = ".SEG";
        private static List<Tuple<int, Segment>> _segmentCache;

        private static Random _random;

        static Segments()
        {
            SegmentsPath = Directory.GetCurrentDirectory() + @"\Segments";
            _segmentCache = new List<Tuple<int, Segment>>();
            _random = new Random();

            Directory.CreateDirectory(SegmentsPath);
        }

        public static async Task<Segment> GetAsync(int segmentId)
        {
            var segmentTuple = _segmentCache.FirstOrDefault(s => s.Item1 == segmentId);
            if (segmentTuple != null) return segmentTuple.Item2;
            var segment = await LoadSegmentFileAsync(segmentId);
            if (_segmentCache.Count >= 1000) _segmentCache.RemoveAt(_random.Next(_segmentCache.Count));
            _segmentCache.Add(new Tuple<int, Segment>(segmentId, segment));
            return segment;
        }

        public static async Task SetAsync(int segmentId, Segment segment)
        {
            var segmentTuple = _segmentCache.FirstOrDefault(s => s.Item1 == segmentId);
            if (segmentTuple != null)
            {
                _segmentCache.Remove(segmentTuple);
                _segmentCache.Add(new Tuple<int, Segment>(segmentId, segment));
            }

            await SaveSegmentFileAsync(segmentId, segment);
        }

        public static async Task NewSegmentAsync(int segmentId)
        {
            var segment = new Segment();
            if (_segmentCache.Count >= 1000) _segmentCache.RemoveAt(_random.Next(_segmentCache.Count));
            _segmentCache.Add(new Tuple<int, Segment>(segmentId, segment));
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
            await streamWriter.WriteAsync((string) fileText);
            streamWriter.Close();
        }

        public static async Task DeleteSegmentAsync(int segmentId)
        {
            await Task.Run(() => DeleteSegment(segmentId));
        }

        private static void DeleteSegment(int segmentId)
        {
            var segmentTuple = _segmentCache.FirstOrDefault(s => s.Item1 == segmentId);
            _segmentCache.Remove(segmentTuple);
            File.Delete(SegmentsPath + "\\" + segmentId + SegmentExtension);
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

        private static string SerializeSegment(Segment segment)
        {
            var serSeg = new SerializableSegment();

            var blocks = segment.Blocks;
            for (var y = 0; y < blocks.GetLength(1); y++)
            {
                for (var x = 0; x < blocks.GetLength(0); x++)
                {
                    var block = blocks[x, y];
                    if (block == null) continue;

                    serSeg.BlocksList.Add(new Tuple<int, int, BlockType>(x, y, block.BlockType));

                }
            }

            serSeg.Resources = segment.ResourcePerTick();

            return JsonConvert.SerializeObject(serSeg);
        }

        private static Segment DeserializeSegment(string fileText)
        {
            var deserialized = JsonConvert.DeserializeObject<SerializableSegment>(fileText);
            var segment = new Segment();
            var blocks = segment.Blocks;
            foreach (var tuple in deserialized.BlocksList)
            {
                blocks[tuple.Item1, tuple.Item2] = TicTacZapExtensions.NewBlock(tuple.Item3);
            }

            segment.SetResources(deserialized.Resources);
            return segment;
        }
    }
}
