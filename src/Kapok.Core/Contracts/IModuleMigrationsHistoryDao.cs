using Kapok.BusinessLayer;
using Kapok.Core.DataModel;

namespace Kapok.Core.BusinessLayer;

public interface IModuleMigrationsHistoryDao : IDao<ModuleMigrationsHistory>
{
    ModuleMigrationsHistory New(string moduleName, Module.Migration migration);

    ModuleMigrationsHistory? Find(string moduleName, Module.Migration migration);
}