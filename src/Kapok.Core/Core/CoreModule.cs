using Kapok.Core.BusinessLayer;
using Kapok.Core.DataModel;
using Kapok.Data;
using Kapok.Module;

namespace Kapok.Core;

public sealed class CoreModule : ModuleBase
{
    public CoreModule() : base(nameof(CoreModule))
    {
    }

    public override void Initiate()
    {
        base.Initiate();

        // register entities
        DataDomain.RegisterEntity<ModuleMigrationsHistory, ModuleMigrationsHistoryDao>();
    }
}