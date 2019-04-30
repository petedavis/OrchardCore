using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DynamicResources.Models;
using OrchardCore.DynamicResources.Services;
using OrchardCore.DynamicResources.ViewModels;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.DynamicResources.Controllers
{
    [Admin]
    public class DynamicResourceController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly DynamicResourcesManager _dynamicResourcesManager;
        private readonly ISiteService _siteService;
        private readonly SupportedContentTypeDefinition[] _contentTypeDefinitions;
        private readonly INotifier _notifier;
        
        public DynamicResourceController(
            IAuthorizationService authorizationService,
            DynamicResourcesManager dynamicResourcesManager,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<DynamicResourceController> stringLocalizer,
            IHtmlLocalizer<DynamicResourceController> htmlLocalizer,
            IEnumerable<SupportedContentTypeDefinition> contentTypeDefinitions,
            INotifier notifier)
        {
            _authorizationService = authorizationService;
            _dynamicResourcesManager = dynamicResourcesManager;
            New = shapeFactory;
            _siteService = siteService;
            _contentTypeDefinitions = contentTypeDefinitions.ToArray();
            _notifier = notifier;
            T = stringLocalizer;
            H = htmlLocalizer;
        }

        public dynamic New { get; set; }

        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }

        [AllowAnonymous]
        public async Task<IActionResult> Resource(string name)
        {
            var dynamicResourcesDocument = await _dynamicResourcesManager.GetDynamicResourcesDocumentAsync();

            if (!dynamicResourcesDocument.DynamicResources.ContainsKey(name))
            {
                return NotFound();
            }

            var dynamicResource = dynamicResourcesDocument.DynamicResources[name];

            return new ContentResult { Content = dynamicResource.Content, ContentType = dynamicResource.ContentType };
        }

        public async Task<IActionResult> Index(PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDynamicResources))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var dynamicResourcesDocument = await _dynamicResourcesManager.GetDynamicResourcesDocumentAsync();

            var count = dynamicResourcesDocument.DynamicResources.Count;

            var dynamicResources = dynamicResourcesDocument.DynamicResources.OrderBy(x => x.Key)
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count);

            var model = new DynamicResourceIndexViewModel
            {
                SupportedContentTypes = _contentTypeDefinitions,
                Resources = dynamicResources.Select(x => new DynamicResourceEntry { Name = x.Key, Resource =x.Value }).ToList(),
                Pager = pagerShape
            };

            return View(model);
        }

        public async Task<IActionResult> Create(DynamicResourceViewModel model, string contentType, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDynamicResources))
            {
                return Unauthorized();
            }

            ViewData["ReturnUrl"] = returnUrl;

            var contentTypeDef = _contentTypeDefinitions.FirstOrDefault(x => x.ContentType == contentType);
            if (contentTypeDef == null)
                return NotFound();

            return View(new DynamicResourceViewModel(){ContentTypeDefinition = contentTypeDef});
        }

        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreatePost(DynamicResourceViewModel model, string submit, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDynamicResources))
            {
                return Unauthorized();
            }

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.NameWithoutExtension))
                {
                    ModelState.AddModelError(nameof(DynamicResourceViewModel.NameWithoutExtension), T["The name is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                var dynamicResource = new DynamicResource { Content = model.Content, ContentType = model.ContentType, Description = model.Description };

                var contentTypeDef = _contentTypeDefinitions.First(x => x.ContentType == dynamicResource.ContentType);

                var name = $"{model.NameWithoutExtension}{contentTypeDef.Extension}";

                await _dynamicResourcesManager.UpdateDynamicResourceAsync(name, dynamicResource);
                if (submit != "SaveAndContinue")
                {
                    return RedirectToReturnUrlOrIndex(returnUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string name, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDynamicResources))
            {
                return Unauthorized();
            }

            var dynamicResourcesDocument = await _dynamicResourcesManager.GetDynamicResourcesDocumentAsync();

            if (!dynamicResourcesDocument.DynamicResources.ContainsKey(name))
            {
                return RedirectToAction("Create", new { name, returnUrl });
            }

            var resource = dynamicResourcesDocument.DynamicResources[name];

            var contentTypeDef = _contentTypeDefinitions.FirstOrDefault(x => x.ContentType == resource.ContentType);

            
            var model = new DynamicResourceViewModel
            {
                SourceName = name,
                NameWithoutExtension = Path.GetFileNameWithoutExtension(name),
                Content = resource.Content,
                ContentType = resource.ContentType,
                Description = resource.Description,
                ContentTypeDefinition = contentTypeDef
            };

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string sourceName, DynamicResourceViewModel model, string submit, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDynamicResources))
            {
                return Unauthorized();
            }

            var dynamicResourcesDocument = await _dynamicResourcesManager.GetDynamicResourcesDocumentAsync();

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.NameWithoutExtension))
                {
                    ModelState.AddModelError(nameof(DynamicResourceViewModel.NameWithoutExtension), T["The name is mandatory."]);
                }
            }

            if (!dynamicResourcesDocument.DynamicResources.ContainsKey(sourceName))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var dynamicResource = new DynamicResource { Content = model.Content, ContentType = model.ContentType, Description = model.Description };

                var contentTypeDef = _contentTypeDefinitions.First(x => x.ContentType == dynamicResource.ContentType);

                var name = $"{model.NameWithoutExtension}{contentTypeDef.Extension}";

                await _dynamicResourcesManager.RemoveDynamicResourceAsync(sourceName);
                await _dynamicResourcesManager.UpdateDynamicResourceAsync(name, dynamicResource);
                if (submit != "SaveAndContinue")
                {
                    return RedirectToReturnUrlOrIndex(returnUrl);
                }                
            }

            // If we got this far, something failed, redisplay form
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name, string returnUrl)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageDynamicResources))
            {
                return Unauthorized();
            }

            var dynamicResourcesDocument = await _dynamicResourcesManager.GetDynamicResourcesDocumentAsync();

            if (!dynamicResourcesDocument.DynamicResources.ContainsKey(name))
            {
                return NotFound();
            }

            await _dynamicResourcesManager.RemoveDynamicResourceAsync(name);

            _notifier.Success(H["DynamicResource deleted successfully"]);
            
            return RedirectToReturnUrlOrIndex(returnUrl);
        }

        private IActionResult RedirectToReturnUrlOrIndex(string returnUrl)
        {
            if ((String.IsNullOrEmpty(returnUrl) == false) && (Url.IsLocalUrl(returnUrl)))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
