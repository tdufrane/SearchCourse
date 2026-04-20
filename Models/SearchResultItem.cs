using Umbraco.Cms.Core.Models.PublishedContent;
namespace SearchCourse.Models
{
    public class SearchResultItem
    {
        public IPublishedContent PublishedItem { get; init; }
        public float Score { get; init; }
    }
}
