using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Tour;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using UmbTrainingSite.Package.Migrations;

namespace UmbTrainingSite.Package
{
    public partial class UmbTrainingSiteComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // disable some of the default core tours since they don't make sense to have when the starter kit is installed
            builder.TourFilters().AddFilter(BackOfficeTourFilter.FilterAlias(ToursToRemoveRegex()));

            builder.AddNotificationHandler<MigrationPlansExecutedNotification, PostMigrationNotificationHandler>();
        }

        [GeneratedRegex("umbIntroCreateDocType|umbIntroCreateContent|umbIntroRenderInTemplate|umbIntroViewHomePage|umbIntroMediaSection")]
        private static partial Regex ToursToRemoveRegex();
    }
}
