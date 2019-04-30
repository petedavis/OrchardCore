using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DynamicResources.Models;
using OrchardCore.Navigation;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.DynamicResources.Services;

namespace OrchardCore.DynamicResources
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<DynamicResourcesManager>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.Add(new ServiceDescriptor(typeof(SupportedContentTypeDefinition), new SupportedContentTypeDefinition() { ContentType = "css", Extension = ".css", ContentMimeType = "text/css" }));
            services.Add(new ServiceDescriptor(typeof(SupportedContentTypeDefinition), new SupportedContentTypeDefinition() { ContentType = "javascript", Extension = ".js", ContentMimeType = "application/javascript" }));
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "OrchardCore.DynamicResources",
                areaName: "OrchardCore.DynamicResources",
                template: "DynamicResources/Resource/{name}",
                defaults: new { controller = "DynamicResource", action = "Resource" }
            );
        }
    }
}
