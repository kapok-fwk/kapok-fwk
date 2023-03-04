using NLog;
using System.Diagnostics;
using Kapok.Core.BusinessLayer;
using Kapok.Core.DataModel;
using Kapok.Data;

namespace Kapok.Module;

public static class ModuleEngine
{
    internal static readonly List<Type> LoadedModulesInternal = new();
    internal static readonly Dictionary<Type, ModuleBase> InitiatedModules = new();

    // ReSharper disable once InconsistentlySynchronizedField
    public static IReadOnlyList<Type> LoadedModules => LoadedModulesInternal;

    private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

    public static void InitiateModule(Type moduleType)
    {
        if (!typeof(ModuleBase).IsAssignableFrom(moduleType))
            throw new ArgumentException($"The module type must be assignable from the type {typeof(ModuleBase).FullName}");

        lock (LoadedModulesInternal)
        {
            if (LoadedModulesInternal.Contains(moduleType))
                return; // module is already initiated; do nothing

            var dependsOnModulePropertyInfo = typeof(ModuleBase).GetProperty(nameof(ModuleBase.DependsOnModule));
            Debug.Assert(dependsOnModulePropertyInfo != null);
            Debug.Assert(dependsOnModulePropertyInfo.GetMethod != null);

            var module = (ModuleBase?) Activator.CreateInstance(moduleType);
            if (module == null)
                throw new NotSupportedException($"The module type {moduleType.FullName} does not have a parameter-less constructor.");

            var dependsOnModule = dependsOnModulePropertyInfo.GetMethod.Invoke(module, Array.Empty<object>());
            if (dependsOnModule is IEnumerable<Type> dependsOnModuleEnumerable)
            {
                foreach (var dependentModuleType in dependsOnModuleEnumerable)
                {
                    if (!LoadedModules.Contains(dependentModuleType))
                    {
                        InitiateModule(dependentModuleType);
                    }
                }
            }

            module.Initiate();
        }
    }

    /// <summary>
    /// Invoke migrations for all initiated modules.
    /// </summary>
    public static void Migrate(IDataDomain dataDomain)
    {
        Logger.Info("Start module migration");

        foreach(var module in InitiatedModules.Values)
        {
            if (module.Migrations != null)
            {
                foreach (var migration in module.Migrations)
                {
                    ModuleMigrationsHistory? dbMigration;
                    using (var scope = dataDomain.CreateScope())
                    {
                        var migrationHistoryDao = scope.GetDao<ModuleMigrationsHistory, IModuleMigrationsHistoryDao>();
                        dbMigration = migrationHistoryDao.Find(module.Name, migration);
                    }

                    if (dbMigration == null)
                    {
                        Logger.Info($"Module {module.Name}: Start migration {migration.GetType().Name}");
                        DoMigration(dataDomain, module, migration);
                        Logger.Info($"Module {module.Name}: End migration {migration.GetType().Name}");
                    }
                }
            }
        }

        Logger.Info("End module migration");
    }

    private static void DoMigration(IDataDomain dataDomain, ModuleBase module, Migration migration)
    {
        try
        {
            using var scope = dataDomain.CreateScope();
            using var trans = scope.BeginTransaction();
            var migrationHistoryDao = scope.GetDao<ModuleMigrationsHistory, IModuleMigrationsHistoryDao>();
            var dbMigration = migrationHistoryDao.New(module.Name, migration);
            migrationHistoryDao.Create(dbMigration);

            // call migration script
            migration.ExecuteUp(scope);

            // save changes to db
            scope.Save();
            trans.Commit();
        }
        catch (Exception e)
        {
            throw new MigrateException(moduleName: module.Name, migration, e);
        }
    }
}