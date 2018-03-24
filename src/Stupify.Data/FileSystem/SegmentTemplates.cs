using System.IO;
using System.Threading.Tasks;
using TicTacZap;

namespace Stupify.Data.FileSystem
{
    internal class SegmentTemplates
    {
        private const string TemplateExtension = ".TEMPLATE";
        private readonly string TemplatePath;

        public SegmentTemplates(string dataDirectory)
        {
            TemplatePath = dataDirectory + @"\Inventories";
            Directory.CreateDirectory(TemplatePath);
        }

        public async Task<Segment> GetAsync(int templateId)
        {
            if (!File.Exists(TemplatePath + $@"\{templateId + TemplateExtension}")) return null;
            
            using (var stream = File.OpenText(TemplatePath + $@"\{templateId + TemplateExtension}"))
            {
                var fileText = await stream.ReadToEndAsync();
                return FileSegments.DeserializeSegment(fileText);
            }
        }

        public async Task SaveAsync(int templateId, Segment segment)
        {
            var fileText = FileSegments.SerializeSegment(segment);
            using (var stream = File.CreateText(TemplatePath + $@"\{templateId + TemplateExtension}"))
            {
                await stream.WriteAsync(fileText);
            }
        }
    }
}