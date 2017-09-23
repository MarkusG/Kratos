using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;

namespace Kratos.TypeReaders
{
    public class TimeSpanReader : TypeReader
    {
        private static Regex _pattern = new Regex(@"(\d+[dhms])", RegexOptions.Compiled);

        public override Task<TypeReaderResult> Read(ICommandContext _, string rawInput, IServiceProvider __)
        {
            var result = TimeSpan.Zero;
            var input = rawInput.ToLower();
            var matches = _pattern.Matches(input)
                                  .Cast<Match>()
                                  .Select(m => m.Value);

            foreach (var match in matches)
            {
                var amount = double.Parse(match.Substring(0, match.Length - 1));
                switch (match[match.Length - 1])
                {
                    case 'd': result = result.Add(TimeSpan.FromDays(amount)); break;
                    case 'h': result = result.Add(TimeSpan.FromHours(amount)); break;
                    case 'm': result = result.Add(TimeSpan.FromMinutes(amount)); break;
                    case 's': result = result.Add(TimeSpan.FromSeconds(amount)); break;
                }
            }

            if (result == TimeSpan.Zero)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse TimeSpan"));
            return Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}
