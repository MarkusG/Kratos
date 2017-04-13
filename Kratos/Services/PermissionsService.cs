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
        public HashSet<string> AllPermissions { get; set; }
        public Dictionary<ulong, HashSet<string>> Permissions { get; set; }

        public Result AddPermission(SocketRole role, string perm)
        {
            // Add wildcard permission
            if (perm.Contains("*")) return AddWildcardPermission(role, perm);

            // Add specific permission
            if (!AllPermissions.Contains(perm))
                return new Result { Type = ResultType.Fail, Message = "Permission not found or invalid format." };
            if (!Permissions.ContainsKey(role.Id))
            {
                var permissions = new HashSet<string>();
                permissions.Add(perm);
                Permissions.Add(role.Id, permissions);
                return new Result { Type = ResultType.Success, Message = "Created permissions set for role and added specified permission." };
            }
            if (!Permissions[role.Id].Add(perm))
                return new Result { Type = ResultType.Warning, Message = "Role already has permission." };

            return new Result { Type = ResultType.Success, Message = "Added specified permission." };
        }

        private Result AddWildcardPermission(SocketRole role, string perm)
        {
            var allSplitPerms = AllPermissions.Select(x => x.Split('.'));
            var splitPerm = perm.Split('.');
            if (splitPerm.All(x => x != "*")) return new Result { Type = ResultType.Fail, Message = "Invalid permission format." }; // Catch foo.bar*
            if (splitPerm.Length > 2) return new Result { Type = ResultType.Fail, Message = "Invalid permission format." }; // Catch *.foo.bar
            var parent = splitPerm[0];
            var child = splitPerm[1];
            bool setCreated = false;
            if (!Permissions.ContainsKey(role.Id))
            {
                Permissions.Add(role.Id, new HashSet<string>());
                setCreated = true;
            }
            if (parent == "*")
            {
                if (child == "*")
                {
                    // Add all existing permissions for all parents and children 
                    foreach (var p in AllPermissions)
                        Permissions[role.Id].Add(p);
                    return new Result { Type = ResultType.Success, Message = setCreated ? "Created permission set for role and added all existing permissions."
                                                                                        : "Added all existing permissions to role." };
                }
                else
                {
                    // Add all child permission matches for all existing parent permissions
                    foreach (var p in allSplitPerms.Where(x => x[1] == child))
                        Permissions[role.Id].Add(AllPermissions.FirstOrDefault(x => x == $"{p[0]}.{p[1]}"));
                    return new Result { Type = ResultType.Success, Message = setCreated ? $"Created permission set for role and added all child permissions matching `{child}`."
                                                                                        : $"Added all child permissions `{child}` to role." };
                }
            }
            else
            {
                // Add all child permissions for the given parent permission
                foreach (var p in allSplitPerms.Where(x => x[0] == parent))
                    Permissions[role.Id].Add(AllPermissions.FirstOrDefault(x => x == $"{p[0]}.{p[1]}"));
                return new Result { Type = ResultType.Success, Message = setCreated ? $"Created permission set for role and added all child permissions of `{parent}`."
                                                                                    : $"Added all child permissions of `{parent}` to role." };
            }
        }

        public Result RemovePermission(SocketRole role, string perm)
        {
            if (!Permissions.ContainsKey(role.Id)) return new Result { Type = ResultType.Warning, Message = "Role has no permission set." };
            if (perm.Contains("*")) return RemoveWildcardPermission(role, perm);
            if (!Permissions[role.Id].Remove(perm)) return new Result { Type = ResultType.Warning, Message = "Role does not have permission." };
            return new Result { Type = ResultType.Success, Message = "Permission removed successfully." };
        }

        private Result RemoveWildcardPermission(SocketRole role, string perm)
        {
            var splitPerm = perm.Split('.');
            var rolesSplitPerms = new List<string[]>(Permissions[role.Id].Select(x => x.Split('.')));
            if (splitPerm.All(x => x != "*")) return new Result { Type = ResultType.Fail, Message = "Invalid permission format." }; // Catch foo.bar*
            if (splitPerm.Length > 2) return new Result { Type = ResultType.Fail, Message = "Invalid permission format." }; // Catch *.foo.bar
            var parent = splitPerm[0];
            var child = splitPerm[1];
            if (parent == "*")
            {
                if (child == "*")
                {
                    // Remove all existing permissions for all parents and children
                    Permissions.Remove(role.Id);
                    return new Result { Type = ResultType.Success, Message = "Removed all permissions from role." };
                }
                else
                {
                    // Add all child permission matches for all existing parent permissions
                    foreach (var p in rolesSplitPerms.Where(x => x[1] == child))
                        Permissions[role.Id].Remove(AllPermissions.FirstOrDefault(x => x == $"{p[0]}.{p[1]}"));
                    if (Permissions[role.Id].Count == 0)
                        Permissions.Remove(role.Id);
                    return new Result { Type = ResultType.Success, Message = $"Removed all child permissions `{child}` from role." };
                }
            }
            else
            {
                // Add all child permissions for the given parent permission
                foreach (var p in rolesSplitPerms.Where(x => x[0] == parent))
                    Permissions[role.Id].Remove(AllPermissions.FirstOrDefault(x => x == $"{p[0]}.{p[1]}"));
                if (Permissions[role.Id].Count == 0)
                    Permissions.Remove(role.Id);
                return new Result { Type = ResultType.Success, Message = $"Removed all child permissions of `{parent}` from role." };
            }
        }

        public IEnumerable<string> GetPermissionsForRole(SocketRole role)
        {
            if (Permissions.ContainsKey(role.Id)) return Permissions[role.Id];
            return null;
        }

        public void LoadPermissions(Assembly assembly)
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

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config", "permissions.json")))
                File.Create(Path.Combine(Directory.GetCurrentDirectory(), "config", "permissions.json")).Dispose();
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

            var config = JsonConvert.DeserializeObject<Dictionary<ulong, HashSet<string>>>(serializedConfig);
            if (config == null) return false;

            Permissions = config;

            return true;
        }

        public PermissionsService()
        {
            AllPermissions = new HashSet<string>();
            Permissions = new Dictionary<ulong, HashSet<string>>();
        }
    }
}
