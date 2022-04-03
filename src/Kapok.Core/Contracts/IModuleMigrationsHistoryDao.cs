using Kapok.DataModel;

namespace Kapok.Core;

public interface IModuleMigrationsHistoryDao : IDao<ModuleMigrationsHistory>
{
    ModuleMigrationsHistory New(string moduleName, Kapok.Module.Migration migration);

    ModuleMigrationsHistory? Find(string moduleName, Kapok.Module.Migration migration);
}