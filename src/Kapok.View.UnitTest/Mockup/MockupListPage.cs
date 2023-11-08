using System;

namespace Kapok.View.UnitTest.Mockup;

public class MockupListPage : ListPage<SampleEntity>
{
    public MockupListPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        OpenCardPageAction = new UIOpenReferencedCardPageAction<SampleEntity>("OpenCardPage",
            typeof(MockupCardPage), serviceProvider, DataSet);
    }
}
