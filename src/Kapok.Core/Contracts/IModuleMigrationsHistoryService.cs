﻿using Kapok.BusinessLayer;
using Kapok.Core.DataModel;

namespace Kapok.Core.BusinessLayer;

public interface IModuleMigrationsHistoryService : IEntityService<ModuleMigrationsHistory>
{
    ModuleMigrationsHistory New(string moduleName, Module.Migration migration);

    ModuleMigrationsHistory? Find(string moduleName, Module.Migration migration);
}