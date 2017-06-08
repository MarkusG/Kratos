using System;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Services;

namespace Kratos
{
    public class CommandHandler
    {
        private IServiceProvider _services;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private LocalLogService _localLog;

        public async Task InstallCommandsAsync()
        {
            _commands.Log += _localLog.Log;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m.Channel is SocketGuildChannel)) return; // Ignore DMs
            var message = m as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            var stringPrefix = message.HasStringPrefix("..", ref argPos); // TODO make proper guild-specific prefixes
            var mentionPrefix = message.HasMentionPrefix(_client.CurrentUser, ref argPos);

            if (!stringPrefix && !mentionPrefix) return; // Message isn't trying to invoke a command

            var context = new SocketCommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, argPos, _services);
        }

        public CommandHandler(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetService<DiscordSocketClient>();
            _commands = _services.GetService<CommandService>();
            _localLog = _services.GetService<LocalLogService>();
        }
    }
}
