using System;

namespace Kapok.View.UnitTest.Mockup;

public class MockupCardPage : CardPage<SampleEntity>
{
    public MockupCardPage(IServiceProvider serviceProvider, IDataSetView<SampleEntity> tableData)
        : base(serviceProvider, tableData)
    {
    }
}