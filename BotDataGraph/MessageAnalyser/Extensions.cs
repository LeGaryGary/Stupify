using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotDataGraph.MessageAnalyser
{
    public static class Extensions
    {
        public static IEnumerable<string> ToWords(this string s)
        {
            var separators = s.ToCharArray()
                .Where(char.IsSeparator)
                .Append('-')
                .Distinct()
                .ToArray();
            var punctuation = s
                .Where(char.IsPunctuation)
                .Distinct()
                .ToArray();

            return s.Split(separators).Select(word => word.Trim(punctuation)).Where(word => !string.IsNullOrEmpty(word));
        }
    }
}
