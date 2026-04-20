using Newtonsoft.Json;
using SearchCourse.CustomIndex;
using System.Net;
using Umbraco.Cms.Infrastructure.Examine;
using Examine;

public class TodoIndexPopulator : IndexPopulator
{
    private readonly TodoValueSetBuilder _todoValueSetBuilder;
    public TodoIndexPopulator(TodoValueSetBuilder productValueSetBuilder)
    {
        _todoValueSetBuilder = productValueSetBuilder;
        //We're telling this populator that it's responsible for populating only our index
        RegisterIndex("TodoIndex");
    }
    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        using (WebClient httpClient = new WebClient())
        {
            var jsonData = httpClient.DownloadString("https://jsonplaceholder.typicode.com/todos/");
            var data = JsonConvert.DeserializeObject<IEnumerable<TodoModel>>(jsonData);
            if (data != null)
            {
                foreach (var item in indexes)
                {
                    item.IndexItems(_todoValueSetBuilder.GetValueSets(data.ToArray()));
                }
            }
        }
    }
}