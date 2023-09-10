using Kapok.BusinessLayer;
using Kapok.Core.DataModel;
using Kapok.Data;

namespace Kapok.Core.BusinessLayer;

public class ModuleMigrationsHistoryDao : Dao<ModuleMigrationsHistory>, IModuleMigrationsHistoryDao
{
    public ModuleMigrationsHistoryDao(IDataDomainScope dataDomainScope, IRepository<ModuleMigrationsHistory> repository) : base(dataDomainScope, repository)
    {
    }

    private string TrimModuleName(string moduleName)
    {
        if (moduleName.EndsWith("Module"))
            return moduleName.Substring(0, moduleName.Length);
        return moduleName;
    }

    public ModuleMigrationsHistory New(string moduleName, Module.Migration migration)
    {
        if (moduleName is null)
            throw new ArgumentNullException(nameof(moduleName));
        if (migration is null)
            throw new ArgumentNullException(nameof(migration));

        moduleName = TrimModuleName(moduleName);

        var entity = New();
        entity.ModuleName = moduleName;
        entity.MigrationId = migration.GetType().Name;
        entity.ProductVersion = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? string.Empty;
        return entity;
    }

    public ModuleMigrationsHistory? Find(string moduleName, Module.Migration migration)
    {
        if (moduleName is null)
            throw new ArgumentNullException(nameof(moduleName));
        if (migration is null)
            throw new ArgumentNullException(nameof(migration));

        moduleName = TrimModuleName(moduleName);
        var migrationId = migration.GetType().Name;

        return (
            from e in AsQueryable()
            where e.ModuleName == moduleName &&
                  e.MigrationId == migrationId
            select e
        ).FirstOrDefault();
    }
}