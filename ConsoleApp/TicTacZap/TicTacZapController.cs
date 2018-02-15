using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.DataModels;
using TicTacZap.Segment.Blocks;
using Segment = TicTacZap.Segment.Segment;

namespace StupifyConsoleApp.TicTacZap
{
    public static class TicTacZapController
    {
        private static readonly string Path;
        private const string Extension = ".SEG";

        private static Dictionary<int, Segment> Segments { get; } = new Dictionary<int, Segment>();

        private static Dictionary<int, int?> UserSelection { get; } = new Dictionary<int, int?>();

        static TicTacZapController()
        {
            Path = Directory.GetCurrentDirectory() + @"\Segments";
            Directory.CreateDirectory(Path);

            foreach (var filePath in Directory.GetFiles(Path))
            {
                if (!filePath.EndsWith(Extension, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                
                var substring = filePath.Substring(Path.Length + 1, filePath.Length - (Extension.Length + Path.Length+1));

                var segmentId = int.Parse(substring);
                var fileText = File.ReadAllText(filePath);

                var deserializedSegment = DeserializeSegment(fileText);

                Segments.Add(segmentId, deserializedSegment);
            }
        }

        public static async Task Run()
        {
            try
            {
                var timer = new Stopwatch();
                timer.Start();
                while (true)
                {
                    await UpdateBalances();

                    await timer.Wait(1000);
                }
            }
            catch (Exception e)
            {
                await ClientManager.LogAsync(e.ToString());
                throw;
            }
        }

        public static string RenderSegment(int segmentId)
        {
            var segment = Segments[segmentId];
            return segment.TextRender() + Environment.NewLine + $"Output per tick: {segment.OutputPerTick}";
        }

        public static async Task AddSegment(int segmentId)
        {
            var segment = new Segment();
            Segments.Add(segmentId, segment);
            await SaveSegment(segmentId, segment);
        }

        private static async Task SaveSegment(int segmentId, Segment segment)
        {
            var fileText = SerializeSegment(segment);

            var streamWriter = File.CreateText(Path + $@"\{segmentId + Extension}");
            await streamWriter.WriteAsync(fileText);
            streamWriter.Close();
        }

        public static void DeleteSegment(int segmentId)
        {
            Segments.Remove(segmentId);

            File.Delete(Path+"\\"+segmentId+Extension);
        }

        public static async Task<bool> AddBlock(int segmentId, int x, int y, BlockType blockType)
        {
            var segment = Segments[segmentId];
            var addBlockResult = segment.AddBlock(x, y, blockType);
            await SaveSegment(segmentId, segment);
            return addBlockResult;
        }

        public static async Task<bool> DeleteBlock(int segmentId, int x, int y)
        {
            var segment = Segments[segmentId];
            var deleteBlockResult = segment.DeleteBlock(x, y);
            await SaveSegment(segmentId, segment);
            return deleteBlockResult;
        }

        private static async Task UpdateBalances()
        {
            using (var db = new BotContext())
            {
                foreach (var segment in Segments)
                {
                    try
                    {
                        var dbSegment = await db.Segments.FirstAsync(s => s.SegmentId == segment.Key);
                        var user = await db.Users.FirstAsync(u => u.UserId == dbSegment.UserId);
                        user.Balance += segment.Value.OutputPerTick;
                        await db.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        await ClientManager.LogAsync(e.ToString());
                        throw;
                    }
                }
            }
            
        }

        private static async Task Wait(this Stopwatch timer, int tickMinTime)
        {
            while (true)
            {
                if (timer.ElapsedMilliseconds > tickMinTime)
                {
                    timer.Restart();
                    break;
                }

                await Task.Delay(50);
            }
        }

        public static decimal GetSegmentOutput(int segmentId)
        {
            return Segments[segmentId].OutputPerTick;
        }

        private static string SerializeSegment(Segment segment)
        {
            var serSeg = new SerializableSegment();

            var blocks = segment.Blocks;
            for (var y = 0; y < blocks.GetLength(1); y++)
            {
                for (var x = 0; x < blocks.GetLength(0); x++)
                {
                    var block = blocks[x,y];
                    if(block == null) continue;

                    serSeg.BlocksList.Add(new Tuple<int, int, BlockType>(x, y, block.Type));

                }
            }

            serSeg.OutputPerTick = segment.OutputPerTick;
            return JsonConvert.SerializeObject(serSeg);
        }

        private static Segment DeserializeSegment(string fileText)
        {
            var deserialized = JsonConvert.DeserializeObject<SerializableSegment>(fileText);
            var segment = new Segment();
            var blocks = segment.Blocks;
            foreach (var tuple in deserialized.BlocksList)
            {
                blocks[tuple.Item1, tuple.Item2] = Segment.NewBlock(tuple.Item3);
            }

            blocks[4, 4] = segment.Controller;
            segment.OutputPerTick = deserialized.OutputPerTick;
            return segment;
        }

        public static bool SetUserSegmentSelection(int userId, int segmentId)
        {
            if (!UserSelection.ContainsKey(userId)) UserSelection.Add(userId, null);
            if (!Segments.ContainsKey(segmentId)) return false;
            UserSelection[userId] = segmentId;
            return true;

        }

        public static int? GetUserSelection(int userId)
        {
            if (UserSelection.ContainsKey(userId)) return UserSelection[userId];
            UserSelection.Add(userId, null);
            return null;
        }
    }
}
