using System;
using System.Collections.Generic;
using System.Linq;
using Kapok.BusinessLayer;
using Kapok.Module;
using Kapok.View.UnitTest.DataModel;
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
        var currentPage = new ListPage<SampleEntity>(ViewDomain, DataDomain);
        currentPage.DataSet.Load();
        currentPage.DataSet.Add(new SampleEntity
        {
            Id = Guid.Empty,
            Name = "Test Row"
        });
        currentPage.DataSet.Current = currentPage.DataSet.Collection.FirstOrDefault();
        Assert.NotNull(currentPage.DataSet.Current);

        var action =
            new UIOpenReferencedPageAction<SampleEntity>("OpenListPage", typeof(ListPage<SampleEntity>), ViewDomain,
                baseDataSetView: currentPage.DataSet,
                filter: (filterSet, entry, _) =>
                {
                    var filter = (IFilterSet<SampleEntity>)filterSet;
                    
                    filter.AddPropertyFilter(nameof(SampleEntity.Name), entry.Name);
                });

        action.Execute(new List<SampleEntity?> { currentPage.DataSet.Current });
    }
}