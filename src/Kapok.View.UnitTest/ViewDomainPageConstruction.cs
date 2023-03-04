using System.Collections.Generic;
using System.Diagnostics;
using Kapok.Data;
using Kapok.Data.InMemory;
using Kapok.Entity;
using Xunit;

namespace Kapok.View.UnitTest;

/// <summary>
/// Tests the page construction from the view domain.
/// </summary>
public class ViewDomainPageConstruction
{
    public UnitTestViewDomain ViewDomain { get; }
    public IDataDomain DataDomain { get; }

    public ViewDomainPageConstruction()
    {
        ViewDomain = new UnitTestViewDomain();

        Data.DataDomain.RegisterEntity<SampleEntity>();
        DataDomain = new InMemoryDataDomain();
    }

    public class SampleEntity : EditableEntityBase
    {
#pragma warning disable CS8618
        public string SampleProperty { get; set; }
#pragma warning restore CS8618
    }

    public class SampleListPage : ListPage<SampleEntity>
    {
        public SampleListPage(IViewDomain? viewDomain, IDataDomainScope? dataDomainScope = null)
            : base(viewDomain, dataDomainScope)
        {
            OpenCardPageAction =
                new UIOpenReferencedCardPageAction<SampleEntity>("OpenCardPage", typeof(SampleCardPage), ViewDomain,
                    DataSet);
        }
    }

    public class SampleCardPage : CardPage<SampleEntity>
    {
        public SampleCardPage(IDataSetView<SampleEntity> dataSet, IViewDomain viewDomain, IDataDomainScope dataDomainScope)
            : base(dataSet, viewDomain, dataDomainScope)
        {
            Assert.NotNull(dataSet);
            Assert.NotNull(viewDomain);
            Assert.NotNull(dataDomainScope);
        }
    }

    [Fact]
    public void BuildThroughOpenCardPageAcgion()
    {
        using var scope = DataDomain.CreateScope();

        var listPage = new SampleListPage(ViewDomain, scope);

        var entity = new SampleEntity
        {
            SampleProperty = "Sample data"
        };

        Assert.NotNull(listPage.DataSet);
        Debug.Assert(listPage.DataSet != null);

        listPage.DataSet.Load();
        listPage.DataSet.Add(entity);

        // test if the construction of the card page works here
        Assert.NotNull(listPage.OpenCardPageAction);
        Debug.Assert(listPage.OpenCardPageAction != null);
        listPage.OpenCardPageAction.Execute(new List<SampleEntity?> {entity });

        Assert.False(ViewDomain.HasErrors);
    }
}