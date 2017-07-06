using Discord.Commands;
using Kratos.Results;

namespace Kratos.Results
{
    public class SimpleRuntimeResult : RuntimeResult
    {
        public ResultType Type { get; set; }

        public static SimpleRuntimeResult FromSuccess(string reason = null) =>
            new SimpleRuntimeResult(null, reason);

        public static SimpleRuntimeResult FromWarning(string reason) =>
            new SimpleRuntimeResult(null, reason, ResultType.Warning);

        public static SimpleRuntimeResult FromFailure(string reason) =>
            new SimpleRuntimeResult(CommandError.Unsuccessful, reason, ResultType.Failure);

        private SimpleRuntimeResult(CommandError? error, string reason, ResultType type) : base(error, reason) { }

        public SimpleRuntimeResult(CommandError? error, string reason) : base(error, reason) { }

        public override string ToString()
        {
            switch (Type)
            {
                case ResultType.Success: return $"🆗 {Reason}";
                case ResultType.Warning: return $"⚠ {Reason}";
                case ResultType.Failure: return $"❌ {Reason}";
                default: return null;
            }
        }
    }
}
