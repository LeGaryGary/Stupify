using System.IO;
using System.Threading.Tasks;
using TicTacZap;

namespace Stupify.Data.FileSystem
{
    internal class SegmentTemplates
    {
        private const string TemplateExtension = ".TEMPLATE";
        private readonly string _templatePath;

        public SegmentTemplates(string dataDirectory)
        {
            _templatePath = dataDirectory + @"\Inventories";
            Directory.CreateDirectory(_templatePath);
        }

        public async Task<Segment> GetAsync(int templateId)
        {
            if (!File.Exists(_templatePath + $@"\{templateId + TemplateExtension}")) return null;
            
            using (var stream = File.OpenText(_templatePath + $@"\{templateId + TemplateExtension}"))
            {
                var fileText = await stream.ReadToEndAsync().ConfigureAwait(false);
                return FileSegments.DeserializeSegment(fileText);
            }
        }

        public async Task SaveAsync(int templateId, Segment segment)
        {
            var fileText = FileSegments.SerializeSegment(segment);
            using (var stream = File.CreateText(_templatePath + $@"\{templateId + TemplateExtension}"))
            {
                await stream.WriteAsync(fileText).ConfigureAwait(false);
            }
        }
    }
}