using System.IO;
using System.Threading.Tasks;
using TicTacZap;

namespace Stupify.Data.FileSystem
{
    public static class SegmentTemplates
    {
        private const string TemplateExtension = ".TEMPLATE";
        private static readonly string TemplatePath;

        static SegmentTemplates()
        {
            TemplatePath = Config.DataDirectory + @"\Inventories";
            Directory.CreateDirectory(TemplatePath);
        }

        public static async Task<Segment> GetAsync(int templateId)
        {
            if (!File.Exists(TemplatePath + $@"\{templateId + TemplateExtension}")) return null;
            var fileText = await File.ReadAllTextAsync(TemplatePath + $@"\{templateId + TemplateExtension}");
            return FileSegments.DeserializeSegment(fileText);
        }

        public static async Task SaveAsync(int templateId, Segment segment)
        {
            var fileText = FileSegments.SerializeSegment(segment);
            await File.WriteAllTextAsync(TemplatePath + $@"\{templateId + TemplateExtension}", fileText);
        }
    }
}