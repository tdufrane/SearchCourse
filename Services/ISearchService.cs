using Examine;
using SearchCourse.Models;
namespace SearchCourse.Services
{
    public interface ISearchService
    {
        IEnumerable<ISearchResult> GetSearchResults(string searchTerm, string contentType, out long totalItemCount);
        IEnumerable<SearchResultItem> GetContentSearchResults(string searchTerm, string contentType, out long totalItemCount);
    }
}