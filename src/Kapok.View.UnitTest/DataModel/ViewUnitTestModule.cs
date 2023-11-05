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
        Data.DataDomain.RegisterEntity<SampleEntity>();
    }
}