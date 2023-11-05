using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kapok.BusinessLayer;
using Kapok.Core.UnitTest.Filter;
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
            },
            Filter = new Filter<SampleEntity>(e => e.Name.StartsWith("Jack"))
        };

        listView.EntityType = typeof(SampleEntity);

        var jsonString = JsonSerializer.Serialize(listView, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters =
            {
                new IFilterJsonConverter()
            }
        });

        Assert.NotNull(jsonString);
    }

    [Fact]
    public void DeserializeTest()
    {
        string jsonString = @"{""Name"":""Standard"",""DisplayName"":[{""Key"":""en-US"",""Value"":""Standard""}],""Columns"":[{""Width"":null,""IsFilterable"":true,""IsHidden"":false,""ShowHierarchicalTree"":false,""TextWrap"":false,""Name"":""Id"",""IsReadOnly"":true,""ArrayIndex"":null,""LookupDefinition"":null,""DrillDownDefinition"":null,""DisplayShortName"":null,""DisplayName"":null,""DisplayDescription"":null,""StringFormat"":null,""NullDisplayText"":null},{""Width"":null,""IsFilterable"":true,""IsHidden"":false,""ShowHierarchicalTree"":false,""TextWrap"":false,""Name"":""Name"",""IsReadOnly"":false,""ArrayIndex"":null,""LookupDefinition"":null,""DrillDownDefinition"":null,""DisplayShortName"":null,""DisplayName"":null,""DisplayDescription"":null,""StringFormat"":null,""NullDisplayText"":null}],""SortDirection"":null,""Filter"":{""Context"":{""Members"":[[""P"",0,""Name"",[],1],[""M"",1,""StartsWith"",[1],2]],""Types"":[[""::"",""Kapok.View.UnitTest.SampleEntity"",0],[""::"",""System.String"",1],[""::"",""System.Boolean"",1],[""::"",""System.Func\u00602"",1],[""\u003C\u003E"",3,[0,2]]],""Assemblies"":[""Kapok.View.UnitTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=fb21470c71926024"",""System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e""],""Version"":""0.9.0.0""},""Expression"":[""=\u003E"",4,["".()"",1,[""."",0,[""$"",0,0]],[["":"",""Jack"",1]]],[[0,""e""]]]}}";

        var listView = JsonSerializer.Deserialize<DataSetListView>(jsonString, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters =
            {
                new IFilterJsonConverter()
            }
        });

        Assert.NotNull(listView);
        Assert.Equal("Standard", listView.Name);
        Assert.NotNull(listView.DisplayName);
        Assert.Equal("Standard", listView.DisplayName["en-US"]);
        Assert.NotNull(listView.Columns);
        Assert.NotEmpty(listView.Columns);
        Assert.Null(listView.SortDirection);
        Assert.NotNull(listView.Filter);
        Assert.NotNull(listView.Filter.FilterExpression);
    }
}
