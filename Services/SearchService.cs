using Examine;
using Examine.Search;
using SearchCourse.Models;
using System.Diagnostics;
using System.Globalization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;

namespace SearchCourse.Services
{
    public class SearchService : ISearchService
    {
        private readonly IExamineManager _examineManager;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IProfiler _profiler;
        public SearchService(IExamineManager examineManager, IUmbracoContextAccessor umbracoContextAccessor, IProfiler profiler)
        {
            _examineManager = examineManager;
            _umbracoContextAccessor = umbracoContextAccessor;
            _profiler = profiler;
        }
        public IEnumerable<SearchResultItem> GetContentSearchResults(string searchTerm, string contentType, out long totalItemCount)
        {
            var pageOfResults = GetSearchResults(searchTerm, contentType, out totalItemCount);
            var items = new List<SearchResultItem>();
            if (pageOfResults != null && pageOfResults.Any())
            {
                foreach (var item in pageOfResults)
                {
                    if (_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
                    {
                        var page = umbracoContext.Content.GetById(int.Parse(item.Id));
                        if (page != null)
                        {
                            //var page = umbracoContext.Content.GetById(int.Parse(item.Id));
                            var pageMedia = umbracoContext.Media.GetById(int.Parse(item.Id));
                            if (page != null)
                            {
                                items.Add(new SearchResultItem()
                                {
                                    PublishedItem = page,
                                    Score = item.Score
                                });
                            }
                            if (pageMedia != null)
                            {
                                items.Add(new SearchResultItem()
                                {
                                    PublishedItem = pageMedia,
                                    Score = item.Score
                                });
                            }
                        }
                    }
                }
            }
            return items;
        }

        public IEnumerable<ISearchResult> GetSearchResults(string searchTerm, string contentType, out long totalItemCount)
        {
            totalItemCount = 0;
            if (_examineManager.TryGetSearcher("MultiSearcher", out var multiSearcher))
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return Array.Empty<ISearchResult>();
                }
                var fieldToSearchLang = "contents" + "_" + CultureInfo.CurrentCulture.ToString().ToLower();
                var fieldToSearchInvariant = "contents";
                var hideFromNavigation = "umbracoNaviHide";
                var pdfTextContent = "fileTextContent";
                var fieldsToSearch = new[] { fieldToSearchLang, fieldToSearchInvariant, pdfTextContent };
                var criteria = multiSearcher.CreateQuery(null, BooleanOperation.Or);
                //var examineQuery = criteria.GroupedOr(new[] { fieldToSearch, "contents" }, searchTerm.MultipleCharacterWildcard());
                var examineQuery = criteria.GroupedOr(new[] { fieldToSearchLang, "contents", pdfTextContent }, searchTerm.Fuzzy(1));
                examineQuery.Not().Field(hideFromNavigation, 1.ToString());
                examineQuery.Or().Field("__NodeTypeAlias", "blog".Boost(10f));
                examineQuery.OrderByDescending(new SortableField("publishingDate", SortType.Long));
                if (contentType != "All")
                {
                    examineQuery.And().Field("__NodeTypeAlias", contentType);
                }
                using (_profiler.Step("Examine query"))
                {
                    var results = examineQuery.Execute();
                    totalItemCount = results.TotalItemCount;
                    if (results.Any())
                    {
                        return results;
                    }
                    else
                    {
                        Console.WriteLine("Error");
                    }
                }
            }
            return Enumerable.Empty<ISearchResult>();
        }

    }
}
