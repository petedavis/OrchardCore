using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.DynamicResources.Models;
using OrchardCore.Environment.Cache;
using YesSql;

namespace OrchardCore.DynamicResources.Services
{
    public class DynamicResourcesManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly ISession _session;

        private const string CacheKey = nameof(DynamicResourcesManager);

        public DynamicResourcesManager(IMemoryCache memoryCache, ISignal signal, ISession session)
        {
            _memoryCache = memoryCache;
            _signal = signal;
            _session = session;
        }

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        public async Task<DynamicResourcesDocument> GetDynamicResourcesDocumentAsync()
        {
            if (!_memoryCache.TryGetValue(CacheKey, out DynamicResourcesDocument document))
            {
                document = await _session.Query<DynamicResourcesDocument>().FirstOrDefaultAsync();

                if (document == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(CacheKey, out document))
                        {
                            document = new DynamicResourcesDocument();

                            _session.Save(document);
                            _memoryCache.Set(CacheKey, document);
                            _signal.SignalToken(CacheKey);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(CacheKey, document);
                    _signal.SignalToken(CacheKey);
                }
            }

            return document;
        }

        public async Task RemoveDynamicResourceAsync(string name)
        {
            var document = await GetDynamicResourcesDocumentAsync();

            document.DynamicResources.Remove(name);
            _session.Save(document);

            _memoryCache.Set(CacheKey, document);
            _signal.SignalToken(CacheKey);
        }
        
        public async Task UpdateDynamicResourceAsync(string name, DynamicResource template)
        {
            var document = await GetDynamicResourcesDocumentAsync();

            document.DynamicResources[name] = template;
            _session.Save(document);

            _memoryCache.Set(CacheKey, document);
            _signal.SignalToken(CacheKey);
        }

    }
}
