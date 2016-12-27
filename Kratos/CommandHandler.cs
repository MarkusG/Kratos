using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Configs;

namespace Kratos
{
    public class CommandHandler
    {
        private IDependencyMap _map;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private CoreConfig _config;

        public CommandHandler(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
            _config = map.Get<CoreConfig>();
            _commands = new CommandService();
            map.Add(_commands);
            _map = map;
        }

        public async Task InstallAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommandAsync(SocketMessage m)
        {
            var message = m as SocketUserMessage;
            if (message == null) return;
            if (!(message.Channel is IGuildChannel)) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (message.HasStringPrefix(_config.Prefix, ref argPos) || _config.MentionPrefixEnabled(message, _client, ref argPos))
            {
                // Create a Command Context
                var context = new CommandContext(_client, message);
                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed succesfully)
                var result = await _commands.ExecuteAsync(context, argPos, _map);
                if (!result.IsSuccess)
                {
                    if (result is ExecuteResult)
                    {
                        var errorResult = (ExecuteResult)result;
                        Console.WriteLine(errorResult.Exception);
                    }
                    if (result is PreconditionResult)
                    {
                        var preconditionResult = (PreconditionResult)result;
                        await m.Channel.SendMessageAsync($":x: {preconditionResult.ErrorReason}");
                    }
                }
            }
        }
    }
}
