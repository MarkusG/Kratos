using Kratos.Services;

namespace Kratos.Configs
{
    public class AliasTrackingConfig
    {
        public bool Enabled { get; set; }

        public AliasTrackingConfig()
        {

        }

        public AliasTrackingConfig(AliasTrackingService service)
        {
            Enabled = service.Enabled;
        }
    }
}
