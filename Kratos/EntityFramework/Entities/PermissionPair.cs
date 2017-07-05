using System.ComponentModel.DataAnnotations;

namespace Kratos.EntityFramework
{
    public class PermissionPair
    {
        [Key]
        public ulong Id { get; set; }

        public string Permissions { get; set; }
    }
}
