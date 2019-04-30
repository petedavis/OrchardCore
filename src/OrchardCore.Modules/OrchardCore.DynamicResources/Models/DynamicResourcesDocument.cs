using System.Collections.Generic;

namespace OrchardCore.DynamicResources.Models
{
    public class DynamicResourcesDocument
    {
        public int Id { get; set; } // An identifier so that updates don't create new documents

        public Dictionary<string, DynamicResource> DynamicResources { get; } = new Dictionary<string, DynamicResource>();
    }

    public class DynamicResource
    {
        public string Content { get; set; }
        public string ContentType { get; set; }
        public string Description { get; set; }
    }

    public class SupportedContentTypeDefinition
    {
        public string ContentType { get; set; }

        public string ContentMimeType { get; set; }

        public string Extension { get; set; }
    }
}