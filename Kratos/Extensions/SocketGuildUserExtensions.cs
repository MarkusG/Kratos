using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

namespace Kratos.Extensions
{
    public static class SocketGuildUserExtensions
    {
        public static string GetFullName(this SocketGuildUser user)
        {
            if (user.Nickname == null)
                return $"{user.Username}#{user.Discriminator} ({user.Id})";
            else
                return $"{user.Nickname} ({user.Username}#{user.Discriminator}) ({user.Id})";
        }
    }
}
