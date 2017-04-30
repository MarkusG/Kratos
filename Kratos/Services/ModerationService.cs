using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Kratos.Services
{
    public class ModerationService
    {
        public string MuteMessage { get; set; } = "You've been muted in {g} for {t} for the following reason:\n```{r}```";

        public string UnmuteMessage { get; set; } = "You've been unmuted in {g}.";

        public string TempBanMessage { get; set; } = "You've been temporarily banned from {g} for {t} for the following reason:\n```{r}```";

        public string PermaBanMessage { get; set; } = "You've been permanently banned from {g} for the following reason:\n```{r}```";

        public string SoftBanMessage { get; set; } = "You've been softly banned from {g} for the following reason:\n```{r}```\nNote: A softban is simply a kick with message purging.";

        public async Task SaveConfigurationAsync()
        {
            var serialized = JsonConvert.SerializeObject(this, Formatting.Indented);
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config", "moderation.json")))
                File.Create(Path.Combine(Directory.GetCurrentDirectory(), "config", "moderation.json")).Dispose();
            using (var configStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "config", "moderation.json"), FileMode.Truncate))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    await configWriter.WriteAsync(serialized);
                }
            }
        }

        public async Task LoadConfigurationAsync()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config", "moderation.json"))) return;
            using (var configStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "config", "moderation.json"), FileMode.Truncate))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var serialized = await configReader.ReadToEndAsync();
                    var data = JsonConvert.DeserializeObject<ModerationService>(serialized);
                    if (data == null) return;
                    MuteMessage = data.MuteMessage;
                    UnmuteMessage = data.UnmuteMessage;
                    TempBanMessage = data.TempBanMessage;
                    PermaBanMessage = data.PermaBanMessage;
                    SoftBanMessage = data.SoftBanMessage;
                }
            }
        }
    }
}
