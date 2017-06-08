using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kratos.Configuration
{
    public abstract class ConfigurationBase
    {
        [JsonIgnore]
        protected string Path { get; set; }

        public virtual async Task SaveAsync()
        {
            if (File.Exists(Path))
            { 
                using (var stream = new FileStream(Path, FileMode.Truncate))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        string serialized = JsonConvert.SerializeObject(this, Formatting.Indented);
                        await writer.WriteAsync(serialized);
                    }
                }
            }
            else
            {
                using (var stream = new FileStream(Path, FileMode.CreateNew))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        string serialized = JsonConvert.SerializeObject(this, Formatting.Indented);
                        await writer.WriteAsync(serialized);
                    }
                }
            }
        }

        public abstract Task LoadAsync();

        protected ConfigurationBase(string fileName) =>
            Path = Program.GetConfigurationPath(fileName);
    }
}
