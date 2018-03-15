using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TicTacZap;

namespace StupifyConsoleApp.TicTacZapManagement
{
    public static class UniverseController
    {
        private const string UniverseExtension = ".UNI";
        private static readonly string UniversePath;

        private static readonly Universe Universe;

        static UniverseController()
        {
            UniversePath = Config.DataDirectory + @"\Universes";
            Directory.CreateDirectory(UniversePath);
            if (File.Exists(UniversePath + @"\" + Config.UniverseName + UniverseExtension))
            {
                Universe = LoadUniverseFile();
            }
            else
            {
                Universe = new Universe(1000);
                SaveUniverseFileAsync().GetAwaiter().GetResult();
            }
        }

        public static string RenderTheEntiretyOfCreationAsWeKnowIt()
        {
            return "```" + Universe.RenderTheEntiretyOfCreationAsWeKnowIt() + "```";
        }

        public static async Task<(int, int)> NewSegment(int segmentId)
        {
            var newSegmentCoords = Universe.NewSegment(segmentId);
            await SaveUniverseFileAsync();
            return newSegmentCoords;
        }

        public static async Task<(int, int)> DeleteSegment(int segmentId)
        {
            var coords = Universe.FindSegment(segmentId);
            Universe.DeleteSegment(coords);
            await SaveUniverseFileAsync();
            return coords;
        }

        private static Universe LoadUniverseFile()
        {
            var fileText = File.ReadAllText(UniversePath + @"\" + Config.UniverseName + UniverseExtension);
            return JsonConvert.DeserializeObject<Universe>(fileText);
        }

        private static async Task SaveUniverseFileAsync()
        {
            var fileText = JsonConvert.SerializeObject(Universe);
            await File.WriteAllTextAsync(UniversePath + @"\" + Config.UniverseName + UniverseExtension, fileText);
        }
    }
}