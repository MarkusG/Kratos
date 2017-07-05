using Discord.Commands;

namespace Kratos.Results
{
    public class SimpleRuntimeResult : RuntimeResult
    {
        public SimpleRuntimeResult(CommandError? error, string reason) : base(error, reason) { }

        public override string ToString()
        {
            if (Error == null)
                return $":ok: {Reason}";
            else
                return $":x: {Reason}";
        }
    }
}
