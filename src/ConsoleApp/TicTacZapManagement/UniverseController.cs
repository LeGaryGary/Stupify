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

        private static readonly Universe _universe;

        static UniverseController()
        {
            UniversePath = Config.DataDirectory + @"\Universes";
            Directory.CreateDirectory(UniversePath);
            if (File.Exists(UniversePath + Config.UniverseName + UniverseExtension))
            {
                _universe = LoadUniverseFile();
            }
            else
            {
                _universe = new Universe(10);
                SaveUniverseFileAsync().GetAwaiter().GetResult();
            }
        }

        public static string RenderTheEntiretyOfCreationAsWeKnowIt()
        {
            return "```" + _universe.RenderTheEntiretyOfCreationAsWeKnowIt() + "```";
        }

        public static async Task<(int, int)> NewSegment(int segmentId)
        {
            var newSegmentCoords = _universe.NewSegment(segmentId);
            await SaveUniverseFileAsync();
            return newSegmentCoords;
        }

        public static async Task<(int, int)> DeleteSegment(int segmentId)
        {
            var coords = _universe.FindSegment(segmentId);
            _universe.DeleteSegment(coords);
            await SaveUniverseFileAsync();
            return coords;
        }

        private static Universe LoadUniverseFile()
        {
            var fileText = File.ReadAllText(UniversePath + Config.UniverseName + UniverseExtension);
            return JsonConvert.DeserializeObject<Universe>(fileText);
        }

        private static async Task SaveUniverseFileAsync()
        {
            var fileText = JsonConvert.SerializeObject(_universe);
            await File.WriteAllTextAsync(UniversePath + @"\" + Config.UniverseName + UniverseExtension, fileText);
        }
    }
}