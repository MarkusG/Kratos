using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kratos.Configuration
{
    public class BotConfiguration : ConfigurationBase
    {
        public string Token { get; set; }

        public override async Task LoadAsync()
        {
            if (!File.Exists(Path)) return;
            using (var stream = new FileStream(Path, FileMode.Open))
            {
                using (var reader = new StreamReader(stream))
                {
                    var serialized = await reader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<BotConfiguration>(serialized);
                    Token = config.Token;
                }
            }
        }

        public BotConfiguration() : base("bot.json") { }
    }
}
