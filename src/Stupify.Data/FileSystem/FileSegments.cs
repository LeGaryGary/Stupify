using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TicTacZap;
using TicTacZap.Blocks;

namespace Stupify.Data.FileSystem
{
    internal class FileSegments
    {
        private const string SegmentExtension = ".SEG";
        private readonly string _segmentsPath;
        private readonly List<Tuple<int, Segment>> _segmentCache;
        
        private readonly Random _random;

        public FileSegments(string dataDirectory, Random random)
        {
            _random = random;
            _segmentsPath = dataDirectory + "/Segments";
            _segmentCache = new List<Tuple<int, Segment>>();

            Directory.CreateDirectory(_segmentsPath);
        }

        public async Task<Segment> GetAsync(int segmentId)
        {
            var segmentTuple = _segmentCache.FirstOrDefault(s => s.Item1 == segmentId);
            if (segmentTuple != null) return segmentTuple.Item2;
            var segment = await LoadSegmentFileAsync(segmentId);
            if (_segmentCache.Count >= 1000) _segmentCache.RemoveAt(_random.Next(_segmentCache.Count));
            _segmentCache.Add(new Tuple<int, Segment>(segmentId, segment));
            return segment;
        }

        public async Task SetAsync(int segmentId, Segment segment)
        {
            var segmentTuple = _segmentCache.FirstOrDefault(s => s.Item1 == segmentId);
            if (segmentTuple != null)
            {
                _segmentCache.Remove(segmentTuple);
                _segmentCache.Add(new Tuple<int, Segment>(segmentId, segment));
            }

            await SaveSegmentFileAsync(segmentId, segment);
        }

        public async Task NewSegmentAsync(int segmentId)
        {
            var segment = new Segment();
            if (_segmentCache.Count >= 1000) _segmentCache.RemoveAt(_random.Next(_segmentCache.Count));
            _segmentCache.Add(new Tuple<int, Segment>(segmentId, segment));
            await SaveSegmentFileAsync(segmentId, segment);
        }

        private async Task<Segment> LoadSegmentFileAsync(int segmentId)
        {
            using (var stream = File.OpenText(_segmentsPath + $@"\{segmentId}{SegmentExtension}"))
            {
                var fileText = await stream.ReadToEndAsync();
                return DeserializeSegment(fileText);
            }
        }

        private async Task SaveSegmentFileAsync(int segmentId, Segment segment)
        {
            var fileText = SerializeSegment(segment);

            using (var streamWriter = File.CreateText(_segmentsPath + $@"\{segmentId + SegmentExtension}"))
            {
                await streamWriter.WriteAsync(fileText);
            }
        }

        public void DeleteSegment(int segmentId)
        {
            var segmentTuple = _segmentCache.FirstOrDefault(s => s.Item1 == segmentId);
            if (segmentTuple != null) _segmentCache.Remove(segmentTuple);
            File.Delete(_segmentsPath + "\\" + segmentId + SegmentExtension);
        }

        public async Task<Dictionary<BlockType, int>> ResetSegmentAsync(int segmentId)
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