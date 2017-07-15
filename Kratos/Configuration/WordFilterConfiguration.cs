using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Kratos.Data;

namespace Kratos.Configuration
{
    public class WordFilterConfiguration : ConfigurationBase
    {
        public List<WordFilter> Filters { get; set; } = new List<WordFilter>();

        public override async Task LoadAsync()
        {
            if (!File.Exists(Path)) return;
            using (var stream = new FileStream(Path, FileMode.Open))
            {
                using (var reader = new StreamReader(stream))
                {
                    var serialized = await reader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<WordFilterConfiguration>(serialized);

                    Filters = config.Filters;
                }
            }
        }

        public WordFilterConfiguration() : base("filter.json") { }
    }
}
