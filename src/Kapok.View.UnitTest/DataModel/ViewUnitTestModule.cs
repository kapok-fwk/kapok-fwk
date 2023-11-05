using Kapok.Data;
using Kapok.Module;

namespace Kapok.View.UnitTest.DataModel;

public class ViewUnitTestModule : ModuleBase
{
    public ViewUnitTestModule() : base(nameof(ViewUnitTestModule))
    {
    }

    public override void Initiate()
    {
        base.Initiate();

        // register DataModel
        DataDomain.RegisterEntity<SampleEntity>();
    }
}