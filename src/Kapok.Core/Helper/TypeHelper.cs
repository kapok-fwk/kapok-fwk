using System.Reflection;

namespace Kapok.Core;

public class TypeNotFoundException : Exception
{
    public TypeNotFoundException(string typeFullName)
        // TODO: translation required
        : base($"Type not found by type full name: {typeFullName}")
    {
    }

    public TypeNotFoundException(string typeFullName, Exception innerException)
        // TODO: translation required
        : base($"Type not found by type full name: {typeFullName}", innerException)
    {
    }
}

public static class TypeHelper
{
    // See also: https://stackoverflow.com/questions/12306/can-i-serialize-a-c-sharp-type-object
    public static Type GetTypeFromTypeFullName(string typeFullName)
    {
        var type = Type.GetType(typeFullName);
        if (type != null)
            return type;

        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //To speed things up, we check first in the already loaded assemblies.
            foreach (var assembly in assemblies)
            {
                type = assembly.GetType(typeFullName);
                if (type != null)
                    break;
            }
            if (type != null)
                return type;

            var loadedAssemblies = assemblies.ToList();

            foreach (var loadedAssembly in assemblies)
            {
                foreach (AssemblyName referencedAssemblyName in loadedAssembly.GetReferencedAssemblies())
                {
                    var found = loadedAssemblies.All(x => x.GetName() != referencedAssemblyName);

                    if (!found)
                    {
                        try
                        {
                            var referencedAssembly = Assembly.Load(referencedAssemblyName);
                            type = referencedAssembly.GetType(typeFullName);
                            if (type != null)
                                break;
                            loadedAssemblies.Add(referencedAssembly);
                        }
                        catch
                        {
                            //We will ignore this, because the Type might still be in one of the other Assemblies.
                        }
                    }
                }
            }
        }
        catch (Exception exception)
        {
            throw new TypeNotFoundException(typeFullName, exception);
        }

        if (type == null)
        {
            throw new TypeNotFoundException(typeFullName);
        }

        return type;
    }
}