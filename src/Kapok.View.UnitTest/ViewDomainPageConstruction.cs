using System.Linq;
using Kapok.Core;
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

        Kapok.Core.DataDomain.RegisterEntity<SampleEntity>();
        DataDomain = new InMemoryDataDomain();
    }

    public class SampleEntity : EditableEntityBase
    {
        public string SampleProperty { get; set; }
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

        listPage.DataSet.Load();
        listPage.DataSet.Add(entity);

        // test if the construction of the card page works here
        listPage.OpenCardPageAction.Execute(new[] { entity }.ToList());

        Assert.False(ViewDomain.HasErrors);
    }
}