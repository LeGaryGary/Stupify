using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StupifyConsoleApp.Client;
using StupifyConsoleApp.DataModels;
using Segment = TicTacZap.Segment.Segment;

namespace StupifyConsoleApp.TicTacZap
{
    public static class TicTacZapController
    {
        private static readonly string Path;
        private const string Extension = ".SEG";

        private static Dictionary<int, Segment> Segments { get; } = new Dictionary<int, Segment>();

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

                Segments.Add(segmentId, JsonConvert.DeserializeObject<Segment>(fileText));
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
            return segment.TextRender();
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

        public static async Task AddSegment(int segmentId)
        {
            var segment = new Segment();
            Segments.Add(segmentId, segment);

            var streamWriter = File.CreateText(Path + $@"\{segmentId+Extension}");
            await streamWriter.WriteAsync(JsonConvert.SerializeObject(segment));
            streamWriter.Close();
        }

        public static void DeleteSegment(int segmentId)
        {
            Segments.Remove(segmentId);

            File.Delete(Path+"\\"+segmentId+Extension);
        }
    }
}
