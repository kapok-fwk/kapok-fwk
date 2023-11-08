using System;
using System.Collections.Generic;
using System.Linq;
using Kapok.BusinessLayer;
using Kapok.Module;
using Kapok.View.UnitTest.DataModel;
using Kapok.View.UnitTest.Mockup;
using Xunit;

namespace Kapok.View.UnitTest;

public class UIActionTest : ViewDomainUnitTestBase
{
    static UIActionTest()
    {
        ModuleEngine.InitiateModule(typeof(ViewUnitTestModule));
    }

    [Fact]
    public void ReferencedPageTest()
    {
        var currentPage = new MockupListPage(ServiceProvider);
        currentPage.DataSet.Load();
        currentPage.DataSet.Add(new SampleEntity
        {
            Id = Guid.Empty,
            Name = "Test Row"
        });
        currentPage.DataSet.Current = currentPage.DataSet.Collection.FirstOrDefault();
        Assert.NotNull(currentPage.DataSet.Current);

        var action =
            new UIOpenReferencedPageAction<SampleEntity>("OpenListPage", typeof(ListPage<SampleEntity>), ServiceProvider,
                baseDataSetView: currentPage.DataSet,
                filter: (filterSet, entry, _) =>
                {
                    var filter = (IFilterSet<SampleEntity>)filterSet;
                    
                    filter.AddPropertyFilter(nameof(SampleEntity.Name), entry.Name);
                });

        action.Execute(new List<SampleEntity?> { currentPage.DataSet.Current });
    }

    [Fact]
    public void ReferencedCardPageTest()
    {
        var listPage = new MockupListPage(ServiceProvider);
        listPage.DataSet.Load();
        listPage.DataSet.Add(new SampleEntity
        {
            Id = Guid.Empty,
            Name = "Test Row"
        });

        listPage.OpenCardPageAction.Execute(new List<SampleEntity?>
        {
            listPage.DataSet.Collection.First()
        });
    }
}