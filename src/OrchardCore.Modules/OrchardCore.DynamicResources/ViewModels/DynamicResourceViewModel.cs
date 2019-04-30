using OrchardCore.DynamicResources.Models;

namespace OrchardCore.DynamicResources.ViewModels
{
    public class DynamicResourceViewModel
    {
        public string SourceName { get; set; }
        public string NameWithoutExtension { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }

        public SupportedContentTypeDefinition ContentTypeDefinition { get; set; }
    }
}