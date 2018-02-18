using System.IO;
using System.Threading.Tasks;
using TicTacZap.Segment;

namespace StupifyConsoleApp.TicTacZap
{
    public static class SegmentTemplates
    {
        private const string TemplateExtension = ".TEMPLATE";
        private static readonly string TemplatePath;

        static SegmentTemplates()
        {
            TemplatePath = Directory.GetCurrentDirectory() + @"\Inventories";
            Directory.CreateDirectory(TemplatePath);
        }

        public static async Task<Segment> GetAsync(int templateId)
        {
            if (!File.Exists(TemplatePath + $@"\{templateId + TemplateExtension}")) return null;
            var fileText = await File.ReadAllTextAsync(TemplatePath + $@"\{templateId+TemplateExtension}");
            return Segments.DeserializeSegment(fileText);
        }

        private static async Task SaveAsync(int templateId, Segment segment)
        {
            var fileText = Segments.SerializeSegment(segment);
            await File.WriteAllTextAsync(TemplatePath + $@"\{templateId+TemplateExtension}", fileText);
        }
    }
}
