using System;

namespace Kratos.Data
{
    public class Mute : ModeratorAction
    { 
        public bool Active { get; set; }

        public DateTime UnmuteAt { get; set; }
    }
}
