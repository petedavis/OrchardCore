using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.DynamicResources
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageDynamicResources = new Permission("ManageDynamicResources", "Manage dynamic resources");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] { ManageDynamicResources };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageDynamicResources }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { ManageDynamicResources }
                }
            };
        }
    }
}