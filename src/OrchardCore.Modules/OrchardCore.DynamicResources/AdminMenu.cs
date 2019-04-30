using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.DynamicResources
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Configuration"], content => content
                    .Add(T["Dynamic Resources"], "10", import => import
                        .Action("Index", "DynamicResource", new { area = "OrchardCore.DynamicResources" })
                        .Permission(Permissions.ManageDynamicResources)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
