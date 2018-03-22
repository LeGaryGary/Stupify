using System;
using System.Collections.Generic;

namespace Stupify.Data.SQL.Models
{
    internal class ServerStory
    {
        public int ServerStoryId { get; set; }
        public Server Server { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ServerUser StoryInitiatedBy { get; set; }

        public virtual ICollection<ServerStoryPart> ServerStoryParts { get; set; }
    }
}