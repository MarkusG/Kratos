using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Kratos.Extensions
{
    public static class MatchCollectionExtensions
    {
        public static IEnumerable<Match> ToEnumerable(this MatchCollection matches)
        {
            foreach (var match in matches)
                yield return (Match)match;
        }
    }
}
