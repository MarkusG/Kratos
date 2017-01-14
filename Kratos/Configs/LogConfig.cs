using System;
using System.Collections.Generic;
using System.Text;
using Kratos.Services;

namespace Kratos.Configs
{
    public class LogConfig
    {
        public ulong ModLog { get; set; }

        public ulong ServerLog { get; set; }

        public bool EditsLogged { get; set; }

        public bool DeletesLogged { get; set; }

        public bool JoinsLogged { get; set; }

        public bool LeavesLogged { get; set; }

        public bool NameChangesLogged { get; set; }

        public bool NickChangesLogged { get; set; }

        public LogConfig()
        {

        }

        public LogConfig(LogService s)
        {
            ModLog = s.ModLogChannelId;
            ServerLog = s.ServerLogChannelId;
            EditsLogged = s.EditsLogged;
            DeletesLogged = s.DeletesLogged;
            JoinsLogged = s.JoinsLogged;
            LeavesLogged = s.LeavesLogged;
            NameChangesLogged = s.NameChangesLogged;
            NickChangesLogged = s.NickChangesLogged;
        }
    }
}
