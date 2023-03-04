using System.Diagnostics;

namespace Kapok.Data;

public static class DataDomainScopeUsageManager
{
    private static readonly Dictionary<IDataDomainScope, List<int>> DataDomainScopeUsage = new();

    public static void RegisterUsage(this IDataDomainScope dataDomainScope, object usedInObject)
    {
        if (dataDomainScope == null) throw new ArgumentNullException(nameof(dataDomainScope));
        if (usedInObject == null) throw new ArgumentNullException(nameof(usedInObject));

        if (DataDomainScopeUsage.ContainsKey(dataDomainScope))
        {
            if (DataDomainScopeUsage[dataDomainScope].Contains(usedInObject.GetHashCode()))
            {
                throw new NotSupportedException(
                    "The data domain scope is already registered for this object, you can't register an object twice for usage");
            }

            DataDomainScopeUsage[dataDomainScope].Add(usedInObject.GetHashCode());
        }
        else
        {
            DataDomainScopeUsage.Add(dataDomainScope, new List<int>(new []
            {
                usedInObject.GetHashCode()
            }));
        }
    }
        
    public static void UnregisterUsage(this IDataDomainScope dataDomainScope, object? usedInObject)
    {
        if (dataDomainScope == null) throw new ArgumentNullException(nameof(dataDomainScope));
        if (usedInObject == null) throw new ArgumentNullException(nameof(usedInObject));

        if (!DataDomainScopeUsage.ContainsKey(dataDomainScope))
        {
            Debug.WriteLine("Failed in deregister data domain scope: Data domain scope is not registered.");
            return;
        }

        if (!DataDomainScopeUsage[dataDomainScope].Contains(usedInObject.GetHashCode()))
        {
            Debug.WriteLine("Failed in deregister data domain scope: The data domain has not been registered for this object.");
            return;
        }

        DataDomainScopeUsage[dataDomainScope].Remove(usedInObject.GetHashCode());
        if (DataDomainScopeUsage[dataDomainScope].Count == 0)
        {
            // cleanup scope
            CleanupScope(dataDomainScope);

            // remove scope from the register
            DataDomainScopeUsage.Remove(dataDomainScope);
        }
    }

    private static void CleanupScope(IDataDomainScope dataDomainScope)
    {
        dataDomainScope.Dispose();
    }
}