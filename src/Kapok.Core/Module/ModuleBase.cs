namespace Kapok.Module;

public abstract class ModuleBase
{
    private readonly List<Type> _dependsOnModule = new();

    protected ModuleBase(string name)
    {
        Name = name;
    }

    public IReadOnlyList<Type> DependsOnModule => _dependsOnModule;

    public string Name { get; }

    /// <summary>
    /// Is called when loading the module.
    /// </summary>
    public virtual void Initiate()
    {
        lock (ModuleEngine.LoadedModulesInternal)
        {
            ModuleEngine.LoadedModulesInternal.Add(GetType());
            ModuleEngine.InitiatedModules.Add(GetType(), this);
        }
    }

    protected void AddDependsOnModule(Type moduleType)
    {
        _dependsOnModule.Add(moduleType);
    }

    /// <summary>
    /// A list of module migrations.
    /// </summary>
    public virtual Migration[]? Migrations => null;
}