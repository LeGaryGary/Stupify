using System;

namespace Stupify.Data.SQL.Models
{
    public class ServerStoryPart
    {
        public int ServerStoryPartId { get; set; }
        public ServerStory ServerStory { get; set; }
        public int PartNumber { get; set; }
        public ServerUser PartAuthor { get; set; }
        public DateTime TimeOfAddition { get; set; }
        public string Part { get; set; }
    }
}