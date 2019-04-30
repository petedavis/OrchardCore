using System.Collections.Generic;
using OrchardCore.DynamicResources.Models;

namespace OrchardCore.DynamicResources.ViewModels
{
    public class DynamicResourceIndexViewModel
    {
        public IList<DynamicResourceEntry> Resources { get; set; }
        public dynamic Pager { get; set; }
        public SupportedContentTypeDefinition[] SupportedContentTypes { get; set; }
    }

    public class DynamicResourceEntry
    {
        public string Name { get; set; }
        public DynamicResource Resource { get; set; }
        public bool IsChecked { get; set; }
    }
}