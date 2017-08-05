using System;
using System.Collections.Generic;
using System.Text;

namespace Kratos.EntityFramework
{
    public class TemporaryBanRecord : RecordBase
    {
        public DateTime Expiration { get; set; }

        public bool IsActive { get; set; }
    }
}
