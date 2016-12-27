using System;
using System.Collections.Generic;
using System.Text;

namespace Kratos.Services.Results
{
    public struct Result : IResult
    {
        public ResultType Type { get; set; }

        public string Info { get; set; }
    }
}
