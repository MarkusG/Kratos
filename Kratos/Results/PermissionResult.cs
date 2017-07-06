namespace Kratos.Results
{
    public class PermissionResult
    {
        public ResultType Type { get; set; }

        public string Reason { get; set; }

        public static PermissionResult FromSuccess(string reason) =>
            new PermissionResult { Type = ResultType.Success, Reason = reason };

        public static PermissionResult FromWarning(string reason) =>
            new PermissionResult { Type = ResultType.Warning, Reason = reason };

        public static PermissionResult FromFailure(string reason) =>
            new PermissionResult { Type = ResultType.Failure, Reason = reason };

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
