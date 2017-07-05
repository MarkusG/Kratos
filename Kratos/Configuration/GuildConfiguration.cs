using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Kratos.Data;

namespace Kratos.Configuration
{
    public class GuildConfiguration : ConfigurationBase
    {
        public List<GuildSettings> Guilds { get; set; } = new List<GuildSettings>();

        public GuildSettings GetOrCreate(ulong id)
        {
            var guild = Guilds.FirstOrDefault(g => g.Id == id);
            if (guild == null)
            {
                guild = new GuildSettings { Id = id };
                Guilds.Add(guild);
            }
            return guild;
        }

        public override async Task LoadAsync()
        {
            if (!File.Exists(Path)) return;

            using (var stream = new FileStream(Path, FileMode.Open))
            {
                using (var reader = new StreamReader(stream))
                {
                    var serialized = await reader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<GuildConfiguration>(serialized);
                    Guilds = config.Guilds;
                }
            }
        }

        public GuildConfiguration() : base("guilds.json") { }
    }
}
