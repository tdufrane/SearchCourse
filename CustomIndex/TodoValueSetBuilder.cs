using Examine;
using SearchCourse.CustomIndex;
using Umbraco.Cms.Infrastructure.Examine;

public class TodoValueSetBuilder : IValueSetBuilder<TodoModel>
{
    public IEnumerable<ValueSet> GetValueSets(params TodoModel[] data)
    {
        foreach (var todo in data)
        {
            var indexValues = new Dictionary<string, object>
            {
                ["userId"] = todo.UserId,
                ["id"] = todo.Id,
                ["title"] = todo.Title,
                ["completed"] = todo.Completed
            };
            var valueSet = new ValueSet(todo.Id.ToString(), "todo", indexValues);
            yield return valueSet;
        }
    }
}