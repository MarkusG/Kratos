﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Kratos.Configs
{
    public class RatelimitConfig
    {
        public int Limit { get; set; }

        public int MuteTime { get; set; }

        public bool IsEnabled { get; set; }
    }
}
