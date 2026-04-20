using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using SearchCourse.Models;
using SearchCourse.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace SearchCourse.Controllers
{
    public class SearchPageController : RenderController
    {
        private readonly ISearchService _searchService;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;
        public SearchPageController(
            ILogger<SearchPageController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISearchService searchService,
            IVariationContextAccessor variationContextAccessor,
            ServiceContext context) 
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _searchService = searchService;
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = context;
        }
        public IActionResult SearchPage(string query, string doctypeToSearch)
        {
            //query might be null if we navigate across different language versions of the page
            if (query != null)
            {
                var searchResults = _searchService.GetContentSearchResults(query, doctypeToSearch, out var totalItemCount);
                var searchPageModel = new SearchModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
                {
                    Query = query,
                    DoctypeToSearch = doctypeToSearch,
                    SearchResults = searchResults,
                    TotalResults = totalItemCount
                };
                return CurrentTemplate(searchPageModel);
            }
            else
            {
                var searchPageModel = new SearchModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
                {
                    Query = "_",
                    DoctypeToSearch = "All",
                    SearchResults = Enumerable.Empty<SearchResultItem>(),
                    TotalResults = 0
                };
                return CurrentTemplate(searchPageModel);
            }
        }

    }
}
