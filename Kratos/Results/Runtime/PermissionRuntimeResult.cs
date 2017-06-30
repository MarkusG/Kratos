using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;

namespace Kratos.Results
{
    public class PermissionRuntimeResult : RuntimeResult
    {
        public PermissionResult InnerResult { get; set; }

        public static PermissionRuntimeResult FromInnerResult(PermissionResult inner)
        {
            if (inner.Type != ResultType.Success)
                return new PermissionRuntimeResult(CommandError.Unsuccessful, inner.Reason, inner);
            else
                return new PermissionRuntimeResult(null, inner.Reason, inner);
        }

        public PermissionRuntimeResult(CommandError? error, string reason, PermissionResult inner) : base(error, reason) { }

        public PermissionRuntimeResult() : base(null, null) { }

        public override string ToString()
            => InnerResult.ToString();
    }
}
