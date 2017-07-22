using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Kratos.Preconditions;
using Kratos.Results;
using Kratos.EntityFramework;

namespace Kratos.Services
{
    public class PermissionsService
    {
        public ushort AuthCode { get; set; }

        public ulong MasterId { get; set; }

        public HashSet<string> AllPermissions { get; set; } = new HashSet<string>();

        public async Task<PermissionResult> AddPermissionAsync(ulong id, string permission)
        {
            if (permission.Contains("*"))
                return await AddWildcardPermissionAsync(id, permission);

            if (!AllPermissions.Contains(permission))
                return PermissionResult.FromFailure("Permission not found.");

            using (var context = new KratosContext())
            {
                PermissionResult result;
                await context.Database.EnsureCreatedAsync();
                var pair = await context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
                if (pair != null)
                {
                    var permissions = pair.Permissions.Split(new string[] { ", " } , StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (permissions.Contains(permission))
                        return PermissionResult.FromWarning("Permission already held.");
                    permissions.Add(permission);
                    pair.Permissions = string.Join(", ", permissions);
                    result = PermissionResult.FromSuccess("Permission added.");
                }
                else
                {
                    await context.Permissions.AddAsync(new PermissionPair
                    {
                        Id = id,
                        Permissions = permission
                    });
                    result = PermissionResult.FromSuccess("Set created and permission added.");
                }
                await context.SaveChangesAsync();
                return result;
            }
        }

        public async Task<PermissionResult> AddWildcardPermissionAsync(ulong id, string permission)
        {
            // { foo.bar, hello.world, ... }
            var allSplitPermissions = AllPermissions.Select(p => p.Split('.'));
            // { { foo, bar }, { hello, world }, ... }
            var splitPermission = permission.Split('.');
            // Catch a permission that contains an asterisk that doesn't represent a wild card, such as *foo.bar
            if (splitPermission.All(p => p != "*"))
                return PermissionResult.FromFailure("Invalid format (invalid wildcard usage).");
            // Catch a permission that has too many nodes, such as *.foo.bar
            if (splitPermission.Length > 2)
                return PermissionResult.FromFailure("Invalid format (too many nodes).");

            var parent = splitPermission[0];
            var child = splitPermission[1];

            var addendPermissions = new HashSet<string>();
            if (parent == "*")
            {
                if (child == "*")
                    addendPermissions = AllPermissions;
                else
                {
                    foreach (var p in allSplitPermissions.Where(x => x[1] == child))
                        addendPermissions.Add(AllPermissions.FirstOrDefault(x => x == $"{p[0]}.{p[1]}"));
                }
            }
            else
            {
                foreach (var p in allSplitPermissions.Where(x => x[0] == parent))
                    addendPermissions.Add(AllPermissions.FirstOrDefault(x => x == $"{p[0]}.{p[1]}"));
            }

            using (var context = new KratosContext())
            {
                PermissionResult result;
                await context.Database.EnsureCreatedAsync();
                var pair = await context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
                if (pair != null)
                {
                    var permissions = pair.Permissions.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    var hashPermissions = new HashSet<string>(permissions); // Copy to a Hashset to prevent adding duplicate permissions
                    foreach (var p in addendPermissions)
                        hashPermissions.Add(p);
                    pair.Permissions = string.Join(", ", hashPermissions);
                    result = PermissionResult.FromSuccess("Permissions added.");
                }
                else
                {
                    context.Permissions.Add(new PermissionPair
                    {
                        Id = id,
                        Permissions = string.Join(", ", addendPermissions)
                    });
                    result = PermissionResult.FromSuccess("Set created and permissions added.");
                }
                await context.SaveChangesAsync();
                return result;
            }
        }

        public async Task<PermissionResult> RemovePermissionAsync(ulong id, string permission)
        {
            if (permission.Contains("*"))
                return await RemoveWildcardPermissionAsync(id, permission);

            if (!AllPermissions.Contains(permission))
                return PermissionResult.FromFailure("Permission not found.");

            using (var context = new KratosContext())
            {
                PermissionResult result;
                await context.Database.EnsureCreatedAsync();

                var pair = await context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
                if (pair == null)
                    return PermissionResult.FromWarning("No permissions held by the given role.");

                var permissions = pair.Permissions.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!permissions.Contains(permission))
                    return PermissionResult.FromWarning("Permission not held.");
                permissions.Remove(permission);
                pair.Permissions = string.Join(", ", permissions);

                if (string.IsNullOrWhiteSpace(pair.Permissions))
                {
                    context.Permissions.Remove(pair);
                    result = PermissionResult.FromSuccess("Permission and set removed.");
                }
                else
                    result = PermissionResult.FromSuccess("Permission removed.");
                await context.SaveChangesAsync();
                return result;
            }
        }

        public async Task<PermissionResult> RemoveWildcardPermissionAsync(ulong id, string permission)
        {
            using (var context = new KratosContext())
            {
                PermissionResult result;
                await context.Database.EnsureCreatedAsync();
                var pair = await context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
                if (pair == null)
                    return PermissionResult.FromWarning("No permissions held by the given role.");

                var splitPermission = permission.Split('.');
                // Catch a permission that contains an asterisk that doesn't represent a wild card, such as *foo.bar
                if (splitPermission.All(p => p != "*"))
                    return PermissionResult.FromFailure("Invalid format (invalid wildcard usage).");
                // Catch a permission that has too many nodes, such as *.foo.bar
                if (splitPermission.Length > 2)
                    return PermissionResult.FromFailure("Invalid format (too many nodes).");

                // { foo.bar, hello.world, ... }
                var heldPermissions = pair.Permissions.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                // { { foo, bar }, { hello, world }, ... }
                var splitHeldPermissions = heldPermissions.Select(p => p.Split('.'));

                var parent = splitPermission[0];
                var child = splitPermission[1];

                var subtrahendPermissions = new HashSet<string>();
                if (parent == "*")
                {
                    if (child == "*")
                        subtrahendPermissions = AllPermissions;
                    else
                    {
                        foreach (var p in splitHeldPermissions.Where(x => x[1] == child))
                            subtrahendPermissions.Add(AllPermissions.FirstOrDefault(x => x == $"{p[0]}.{p[1]}"));
                    }
                }
                else
                {
                    foreach (var p in splitHeldPermissions.Where(x => x[0] == parent))
                        subtrahendPermissions.Add(AllPermissions.FirstOrDefault(x => x == $"{p[0]}.{p[1]}"));
                }

                foreach (var p in subtrahendPermissions)
                    heldPermissions.Remove(p);
                pair.Permissions = string.Join(", ", heldPermissions);

                if (string.IsNullOrWhiteSpace(pair.Permissions))
                {
                    context.Permissions.Remove(pair);
                    result = PermissionResult.FromSuccess("Permissions and set removed.");
                }
                else
                    result = PermissionResult.FromSuccess("Permissions removed.");
                await context.SaveChangesAsync();
                return result;
            }
        }

        public async Task<IEnumerable<string>> GetPermissionsAsync(ulong id)
        {
            using (var context = new KratosContext())
            {
                await context.Database.EnsureCreatedAsync();
                var pair = await context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
                if (pair == null)
                    return null;
                return pair.Permissions.Split(new string[] { ", " }, StringSplitOptions.None);
            }
        }

        public async Task<bool> CheckPermissionsAsync(ulong id, string permission)
        {
            if (id == MasterId) return true;

            using (var context = new KratosContext())
            {
                await context.Database.EnsureCreatedAsync();
                var entry = await context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
                if (entry == null)
                    return false;
                var permissions = entry.Permissions.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                return permissions.Contains(permission);
            }
        }

        public async Task LoadPermissionsAsync(Assembly assembly)
        {
            var methods = assembly.GetTypes()
                .SelectMany(x => x.GetMethods());
            foreach (var m in methods)
            {
                var attribute = m.GetCustomAttribute<PermissionAttribute>();
                if (attribute == null) continue;
                AllPermissions.Add(attribute.Permission);
            }

            AllPermissions.Add("automod.bypass");

            var path = Path.Combine(Program.GetOriginalDirectory(), "auth");
            if (!File.Exists(path)) return;
            using (var stream = new FileStream(path, FileMode.Open))
            {
                using (var reader = new StreamReader(stream))
                {
                    var content = await reader.ReadToEndAsync();
                    MasterId = ulong.Parse(content);
                }
            }
        }

        public ushort GenerateAuthCode()
        {
            var rand = new Random();
            AuthCode = (ushort)rand.Next(10000, ushort.MaxValue);
            return AuthCode;
        }

        public async Task AuthAsync(ulong id)
        {
            MasterId = id;
            AuthCode = 0;

            var path = Path.Combine(Program.GetOriginalDirectory(), "auth");
            using (var stream = new FileStream(path, FileMode.Create))
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(MasterId.ToString());
                }
            }
        }
    }
}
