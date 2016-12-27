using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace Kratos.Services
{
    public class UnpunishService
    {
        private bool _running;

        public async Task StartUnpunishLoop()
        {
            _running = true;
            while (_running)
            {
                await Task.Delay(0);
            }
        }
    }
}
