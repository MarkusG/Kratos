using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.WebSocket;
using Newtonsoft.Json;
using Kratos.Data;
using Kratos.Configs;

namespace Kratos.Services
{
    public class AliasTrackingService
    {
        private AliasTrackingContext _db;
        private DiscordSocketClient _client;

        public bool Enabled { get; private set; }

        public async Task SaveConfigurationAsync()
        {
            var config = new AliasTrackingConfig(this);

            var serializedConfig = JsonConvert.SerializeObject(config);

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config", "alias.json")))
                File.Create(Path.Combine(Directory.GetCurrentDirectory(), "config", "alias.json")).Dispose();
            using (var configStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "config", "alias.json"), FileMode.Truncate))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    await configWriter.WriteAsync(serializedConfig);
                }
            }
        }

        public async Task LoadConfigurationAsync()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config", "alias.json"))) return;

            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "config", "alias.json")))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var serializedConfig = await configReader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<AliasTrackingConfig>(serializedConfig);
                    if (config == null) return;

                    if (config.Enabled) Enable();
                }
            }
        }

        public void Enable()
        {
            _client.UserUpdated += _client_UserUpdated;
            _client.GuildMemberUpdated += _client_GuildMemberUpdated;
            Enabled = true;
        }

        public void Disable()
        {
            _client.UserUpdated -= _client_UserUpdated;
            _client.GuildMemberUpdated -= _client_GuildMemberUpdated;
            Enabled = false;
        }

        private async Task _client_GuildMemberUpdated(SocketGuildUser b, SocketGuildUser a)
        {
            if (b.Nickname == a.Nickname || b.Nickname == null) return; // Ignore updates unrelated to nicknames OR new nicknames
            await AddNicknameAsync(new NicknameAlias
            {
                UserId = b.Id,
                GuildId = b.Guild.Id,
                Until = DateTime.UtcNow,
                Alias = b.Nickname
            });
        }

        private async Task _client_UserUpdated(SocketUser b, SocketUser a)
        {
            if (b.Username == a.Username) return; // Ignore updates unrelated to username changes
            await AddUsernameAsync(new UsernameAlias
            {
                UserId = b.Id,
                Until = DateTime.UtcNow,
                Alias = b.Username
            });
        }

        public async Task AddUsernameAsync(UsernameAlias alias)
        {
            if (_db == null)
                _db = new AliasTrackingContext();
            await _db.Database.EnsureCreatedAsync();
            await _db.UsernameAliases.AddAsync(alias);

            await _db.SaveChangesAsync();
        }

        public async Task AddNicknameAsync(NicknameAlias alias)
        {
            if (_db == null)
                _db = new AliasTrackingContext();
            await _db.Database.EnsureCreatedAsync();
            await _db.NicknameAliases.AddAsync(alias);

            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<UsernameAlias>> GetUsernamesAsync(ulong id)
        {
            if (_db == null)
                _db = new AliasTrackingContext();
            await _db.Database.EnsureCreatedAsync();
            return _db.UsernameAliases.Where(u => u.UserId == id)
                                      .OrderBy(u => u.Until);
        }

        public async Task<IEnumerable<NicknameAlias>> GetNicknamesAsync(ulong id)
        {
            if (_db == null)
                _db = new AliasTrackingContext();
            await _db.Database.EnsureCreatedAsync();
            return _db.NicknameAliases.Where(u => u.UserId == id)
                          .OrderBy(u => u.Until);
        }

        public void DisposeContext()
        {
            _db.Dispose();
            _db = null;
        }

        public AliasTrackingService(DiscordSocketClient c)
        {
            _client = c;
        }
    }
}
