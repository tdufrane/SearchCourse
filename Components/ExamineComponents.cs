using Examine;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace SearchCourse.Components
{
    public class ExamineComponents : IComponent
    {
        private readonly IExamineManager _examineManager;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<ExamineComponents> _logger;
        private readonly IContentTypeService _contentTypeService;

        public ExamineComponents(IExamineManager examineManager, ILocalizationService localizationService, IContentTypeService contentTypeService, ILogger<ExamineComponents> logger)
        {
            _examineManager = examineManager;
            _localizationService = localizationService;
            _logger = logger;
            _contentTypeService = contentTypeService;
        }
        public void Initialize()
        {
            if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out var externalIndex))
                throw new InvalidOperationException($"No index found by name {Constants.UmbracoIndexes.ExternalIndexName}");
            if (!(externalIndex is BaseIndexProvider indexProvider))
                throw new InvalidOperationException("Could not cast");
            indexProvider.TransformingIndexValues += IndexProviderTransformingIndexValues;

        }

        private void IndexProviderTransformingIndexValues(object sender, IndexingItemEventArgs e)
        {
            if (e.ValueSet.Category == IndexTypes.Content)
            {
                try
                {
                    IEnumerable<ILanguage> languages = _localizationService.GetAllLanguages();
                    string variesByCulture = null;


                    if (e.ValueSet.Values.TryGetValue("__VariesByCulture", out var result))
                    {
                        variesByCulture = (string)result[0];
                    }
                    ;

                    var updatedValues = e.ValueSet.Values.ToDictionary(x => x.Key, x => x.Value.ToList());

                    if (variesByCulture != null && variesByCulture == "y")
                    {
                        foreach (var language in languages)
                        {
                            var languageIsoCode = language.IsoCode.ToLower();

                            var cultureAndInvariantFields = GetCultureAndInvariantFields(updatedValues, languageIsoCode);
                            var combinedFieldsLang = new StringBuilder();


                            foreach (var field in cultureAndInvariantFields.Where(x => !x.StartsWith("contents") && !x.StartsWith("__Raw")))
                            {
                                updatedValues.TryGetValue(field, out List<object> values);

                                if (values != null)
                                    foreach (var value in values)
                                    {
                                        if (value != null)
                                            combinedFieldsLang.AppendLine(value.ToString());
                                    }
                            }

                            updatedValues.Add("contents_" + languageIsoCode, new List<object> { combinedFieldsLang.ToString() });

                            e.SetValues(updatedValues.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
                        }

                    }
                    else
                    {
                        var combinedFields = new StringBuilder();
                        foreach (var fieldValues in updatedValues)
                        {
                            foreach (var value in fieldValues.Value)
                            {
                                if (value != null)
                                    combinedFields.AppendLine(value.ToString());
                            }
                        }

                        updatedValues.Add("contents", new List<object> { combinedFields.ToString() });

                        e.SetValues(updatedValues.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error combining fields for {ValueSetId}", e.ValueSet.Id);
                }

                try
                {
                    var updatedValues = e.ValueSet.Values.ToDictionary(x => x.Key, x => x.Value.ToList());
                    string[] alias = new string[1] { "blog" };
                    string blogTypeId = _contentTypeService.GetAllContentTypeIds(alias).FirstOrDefault().ToString();
                    updatedValues.TryGetValue("nodeType", out var nType);
                    updatedValues.TryGetValue("publishingDate", out var publishingDate);
                    updatedValues.TryGetValue("createDate", out var fallbackDate);
                    //good to know - dates are saved in DateTime.Ticks
                    //we only do this on Blog doctype
                    if (nType.FirstOrDefault().ToString() == blogTypeId)
                    {
                        if (publishingDate == null)
                        {
                            updatedValues.Remove("publishingDate");
                            //cleared the publishingDate value, let's add one that is filled out
                            updatedValues.Add("publishingDate", fallbackDate);
                        }
                        e.SetValues(updatedValues.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting the fallback for PublishingDate");
                }
            }
        }



        public void Terminate()
        {

        }

        private static IEnumerable<string> GetCultureAndInvariantFields(IDictionary<string, List<object>> values, string culture)
        {
            Regex cultureIsoCodeFieldNameMatchExpression = new Regex("^([_\\w]+)_([a-z]{2}-[a-z0-9]{2,4})$", RegexOptions.Compiled);
            var allFields = values;

            foreach (var field in allFields)
            {
                var match = cultureIsoCodeFieldNameMatchExpression.Match(field.Key);
                if (match.Success && match.Groups.Count == 3 && culture.InvariantEquals(match.Groups[2].Value))
                {
                    yield return field.Key; //matches this culture field
                }
                else if (!match.Success)
                {
                    yield return field.Key; //matches no culture field (invariant)
                }
            }
        }
    }
}