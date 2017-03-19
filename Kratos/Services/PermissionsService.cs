using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Discord.WebSocket;
using Kratos.Services.Results;

namespace Kratos.Services
{
    public class PermissionsService
    {
        public List<string> AllPermissions { get; set; }
        public Dictionary<ulong, List<string>> Permissions { get; set; }

        public Result AddPermission(SocketRole role, string perm)
        {
            bool containsRole = Permissions.ContainsKey(role.Id);
            if (!AllPermissions.Contains(perm))
                return new Result { Type = ResultType.Fail, Info = "Permission not found." };
            if (containsRole && Permissions[role.Id].Contains(perm))
                return new Result { Type = ResultType.Warning, Info = "Role already has permission." };

            var rolePermissions = containsRole ? Permissions[role.Id]
                                               : new List<string>();

            rolePermissions.Add(perm);
            if (containsRole)
                return new Result { Type = ResultType.Success };
            Permissions.Add(role.Id, rolePermissions);
            return new Result { Type = ResultType.Success };
        }

        public PermissionRangeResult AddPermissions(SocketRole role, params string[] perms)
        {
            var failures = new List<string>();
            var warnings = new List<string>();
            var successes = new List<string>();
            bool containsRole = Permissions.ContainsKey(role.Id);
            var rolePermissions = containsRole ? Permissions[role.Id]
                                               : new List<string>();

            foreach (var p in perms)
            {
                if (!AllPermissions.Contains(p))
                {
                    failures.Add(p);
                    continue;
                }
                if (rolePermissions.Contains(p))
                {
                    warnings.Add(p);
                    continue;
                }

                rolePermissions.Add(p);
                successes.Add(p);
            }

            if (!containsRole)
                Permissions.Add(role.Id, rolePermissions);

            return new PermissionRangeResult
            {
                Failures = failures,
                Warnings = warnings,
                Successes = successes,
                Type = failures.Count == 0 && warnings.Count == 0 ? ResultType.Success
                                                                  : ResultType.Warning,
                Info = $"{successes.Count} permissions added successfully. {warnings} permissions were already held by the role. {failures} permissions were not found."
            };
        }

        public Result RemovePermission(SocketRole role, string perm)
        {
            if (!Permissions.ContainsKey(role.Id)) return new Result { Type = ResultType.Warning, Info = "Role has no permissions." };
            if (!Permissions[role.Id].Contains(perm)) return new Result { Type = ResultType.Warning, Info = "Role does not have permission." };
            Permissions[role.Id].Remove(perm);
            return new Result { Type = ResultType.Success };
        }

        public PermissionRangeResult RemovePermissions(SocketRole role, params string[] perms)
        {
            var warnings = new List<string>();
            var successes = new List<string>();

            if (!Permissions.ContainsKey(role.Id)) return new PermissionRangeResult { Type = ResultType.Warning, Info = "Role has no permissions." };

            foreach (var p in perms)
            {
                if (!Permissions[role.Id].Contains(p))
                {
                    warnings.Add(p);
                    continue;
                }

                Permissions[role.Id].Remove(p);
            }

            return new PermissionRangeResult
            {
                Warnings = warnings,
                Successes = successes,
                Type = warnings.Count == 0 ? ResultType.Success
                                           : ResultType.Warning,
                Info = $"{successes.Count} permissions removed sucessfully. {warnings} were not held by the role."
            };
        }

        public void AddPermissions(Assembly assembly)
        {
            var permissions = assembly.GetTypes()
              .SelectMany(x => x.GetMethods())
              .Where(x => x.GetCustomAttributes<Preconditions.RequireCustomPermissionAttribute>().Count() > 0)
              .Select(x => x.GetCustomAttribute<Preconditions.RequireCustomPermissionAttribute>().Permission);
            foreach (var p in permissions)
            {
                if (!AllPermissions.Contains(p))
                    AllPermissions.Add(p);
            }
        }

        public async Task<bool> SaveConfigurationAsync()
        {
            var serializedConfig = JsonConvert.SerializeObject(Permissions, Formatting.Indented);

            using (var configStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "config", "permissions.json"), FileMode.Truncate))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    await configWriter.WriteAsync(serializedConfig);
                    return true;
                }
            }
        }

        public async Task<bool> LoadConfigurationAsync()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config", "permissions.json"))) return false;

            string serializedConfig;

            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "config", "permissions.json")))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    serializedConfig = await configReader.ReadToEndAsync();
                }
            }

            var config = JsonConvert.DeserializeObject<Dictionary<ulong, List<string>>>(serializedConfig);
            if (config == null) return false;

            Permissions = config;

            return true;
        }

        public PermissionsService()
        {
            AllPermissions = new List<string>();
            Permissions = new Dictionary<ulong, List<string>>();
        }
    }
}
