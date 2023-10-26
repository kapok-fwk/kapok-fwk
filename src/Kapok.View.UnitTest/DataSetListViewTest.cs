using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Kapok.View.UnitTest;

public class DataSetListViewTest
{
    [Fact]
    public void SerializeTest()
    {
        var listView = new DataSetListView
        {
            Name = "Standard",
            DisplayName = new Caption
            {
                {"en-US", "Standard"},
            },
            Columns = new List<ColumnPropertyView>
            {
                new(nameof(SampleEntity.Id)),
                new(nameof(SampleEntity.Name)),
            }
        };

        listView.EntityType = typeof(SampleEntity);

        var jsonString = JsonSerializer.Serialize(listView, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        });

        Assert.NotNull(jsonString);
    }
}
