using System;
using System.Collections.Generic;
using System.Linq;

namespace Stupify.Data.Models
{
    public class Story
    {
        private readonly int _minLength;

        public Story(int minLength)
        {
            _minLength = minLength;
        }

        public IEnumerable<string> Parts { get; set; }

        public string Content => Parts.Aggregate(string.Empty, (current, part) => current + part + Environment.NewLine);

        public bool AtLeastMinLength()
        {
            return Parts.Count() >= _minLength;
        }
    }
}

