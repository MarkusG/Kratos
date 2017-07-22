using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using Kratos.Data;


namespace Kratos.Results
{
    public struct WordFilterViolationResult
    {
        public WordFilter Filter { get; }

        public Regex Pattern { get; }

        public MatchCollection Matches { get; }

        public bool Positive { get; }

        public static WordFilterViolationResult FromPositive(WordFilter filter, Regex pattern, MatchCollection matches) =>
            new WordFilterViolationResult(filter, pattern, matches, true);

        public static WordFilterViolationResult FromNegative() =>
            new WordFilterViolationResult(null, null, null, false);

        private WordFilterViolationResult(WordFilter filter, Regex pattern, MatchCollection matches, bool positive)
        {
            Filter = filter;
            Pattern = pattern;
            Positive = positive;
            Matches = matches;
        }
    }
}
