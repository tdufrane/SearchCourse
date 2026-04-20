using Examine;
using SearchCourse.Components;
using SearchCourse.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Infrastructure.Examine;
using UmbracoExamine.PDF;
using SearchCourse.CustomIndex;

namespace SearchCourse.Composers
{
    [ComposeAfter(typeof(ExaminePdfComposer))]
    public class ExamineComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<ISearchService, SearchService>();
            builder.Components().Append<ExamineComponents>();
            builder.Services.ConfigureOptions<ConfigureCustomFieldOptions>();
            builder.Services.AddExamineLuceneMultiSearcher("MultiSearcher", new[] { Constants.UmbracoIndexes.ExternalIndexName, PdfIndexConstants.PdfIndexName });

            builder.Services.AddSingleton<TodoValueSetBuilder>();
            builder.Services.AddSingleton<IIndexPopulator, TodoIndexPopulator>();
            builder.Services
            .AddExamineLuceneIndex<CustomToDoIndex, ConfigurationEnabledDirectoryFactory>("TodoIndex");
        }
    }
}
