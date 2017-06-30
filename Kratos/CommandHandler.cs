using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Services;
using Kratos.Configuration;
using Kratos.Results;

namespace Kratos
{
    public class CommandHandler
    {
        private IServiceProvider _services;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private LocalLogService _localLog;
        private GuildConfiguration _guildsConfig;

        public async Task InstallCommandsAsync()
        {
            _commands.Log += _localLog.LogAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m.Channel is SocketGuildChannel channel)) return; // Ignore DMs
            var message = m as SocketUserMessage;
            if (message == null) return;

            var prefix = _guildsConfig.Guilds.FirstOrDefault(g => g.Id == channel.Guild.Id)?.Prefix;

            int argPos = 0;
            bool stringPrefix;
            if (prefix == null)
                stringPrefix = false;
            else
                stringPrefix = message.HasStringPrefix(prefix, ref argPos);
            var mentionPrefix = message.HasMentionPrefix(_client.CurrentUser, ref argPos);

            if (!stringPrefix && !mentionPrefix) return; // Message isn't trying to invoke a command

            var context = new SocketCommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess)
            {
                if (result.Error.Value != CommandError.UnknownCommand)
                await message.Channel.SendMessageAsync(result.ToString());
            }
        }

        public CommandHandler(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetService<DiscordSocketClient>();
            _commands = _services.GetService<CommandService>();
            _localLog = _services.GetService<LocalLogService>();
            _guildsConfig = _services.GetService<GuildConfiguration>();
        }
    }
}
