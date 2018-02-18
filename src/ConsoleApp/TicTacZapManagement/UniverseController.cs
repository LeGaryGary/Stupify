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

        private static Universe _universe;

        static UniverseController()
        {
            UniversePath = Config.DataDirectory + @"\Universes";
            Directory.CreateDirectory(UniversePath);

            _universe = LoadUniverseFile();
        }

        public static (int, int) NewSegment(int segmentId)
        {
            return _universe.NewSegment(segmentId);
        }

        public static void DeleteSegment(int segmentId)
        {
            (var x, var y) = _universe.FindSegment(segmentId);
            _universe.DeleteSegment(x, y);
        }

        private static Universe LoadUniverseFile()
        {
            var fileText = File.ReadAllText(UniversePath + Config.UniverseName + UniverseExtension);
            return JsonConvert.DeserializeObject<Universe>(fileText);
        }

        private static async Task SaveUniverseFileAsync(Universe universe)
        {
            var fileText = JsonConvert.SerializeObject(universe);
            await File.WriteAllTextAsync(UniversePath + Config.UniverseName + UniverseExtension, fileText);
        }
    }
}
