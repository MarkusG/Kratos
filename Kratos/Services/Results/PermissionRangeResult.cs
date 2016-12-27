using System;
using System.Collections.Generic;
using System.Text;

namespace Kratos.Services.Results
{
    public struct PermissionRangeResult : IResult
    {
        public ResultType Type { get; set; }

        public string Info { get; set; }

        public List<string> Failures { get; set; }

        public List<string> Warnings { get; set; }

        public List<string> Successes { get; set; }
    }
}
