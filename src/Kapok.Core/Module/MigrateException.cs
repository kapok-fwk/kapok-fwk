namespace Kapok.Module;

/// <summary>
/// An exception raised when an exception happens during a migration.
/// </summary>
public class MigrateException : Exception
{
    public string ModuleName { get; }

    public Migration Migration { get; }

    public MigrateException(string moduleName, Migration migration, Exception innerException)
        : base($"An exception occurred while executing migration '{migration.GetType().FullName}' of module '{moduleName}'", innerException)
    {
        ModuleName = moduleName;
        Migration = migration;
    }
}