using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kapok.Data;
using Kapok.Entity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kapok.View.UnitTest;

/// <summary>
/// Tests the page construction from the view domain.
/// </summary>
public class ViewDomainPageConstruction : ViewDomainUnitTestBase
{
    static ViewDomainPageConstruction()
    {
        Data.DataDomain.RegisterEntity<ViewDomainPageConstruction.SampleEntity>();
    }

    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        base.ConfigureServices(serviceCollection);

        serviceCollection.AddDataModelServices();
    }

    public class SampleEntity : EditableEntityBase
    {
#pragma warning disable CS8618
        public string SampleProperty { get; set; }
#pragma warning restore CS8618
    }

    public class SampleListPage : ListPage<SampleEntity>
    {
        public SampleListPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            OpenCardPageAction =
                new UIOpenReferencedCardPageAction<SampleEntity>("OpenCardPage", typeof(SampleCardPage), ServiceProvider,
                    DataSet);
        }
    }

    public class SampleCardPage : CardPage<SampleEntity>
    {
        public SampleCardPage(IServiceProvider serviceProvider, IDataSetView<SampleEntity> dataSet)
            : base(serviceProvider, dataSet)
        {
            Assert.NotNull(serviceProvider);
            Assert.NotNull(dataSet);}
    }

    [Fact]
    public void ConstructSampleListPage()
    {
        using var scope = DataDomain.CreateScope();

        var page = ViewDomain.ConstructPage<SampleListPage>();
        Assert.NotNull(page);

        page = ViewDomain.ConstructPage<SampleListPage>(scope.ServiceProvider);
        Assert.NotNull(page);
    }

    [Fact]
    public void BuildThroughOpenCardPageAction()
    {
        using var scope = DataDomain.CreateScope();

        var listPage = new SampleListPage(ServiceProvider);

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