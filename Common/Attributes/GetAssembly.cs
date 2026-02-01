using System;
using System.Reflection;
using static Arro.Common.Logger;

namespace Arro.Common;

/// <summary>
/// Marks a static field to be automatically populated with a reference to a specific assembly.
/// </summary>
/// <param name="assemblyName">The simple name of the assembly to look for (e.g., "Sims3GameplaySystems").</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class GetAssemblyAttribute(string assemblyName) : Attribute  //Must be defined as public
{
    public string AssemblyName { get; } = assemblyName;
}

internal static class AssemblyChecker
{
    public static void Initialize()
    {
        var fieldsWithAttrs = AttributeCache.GetFieldsWithAttributeEx<GetAssemblyAttribute>();
        if (fieldsWithAttrs.Count == 0) return;

        foreach (var item in fieldsWithAttrs)
        {
            if (item.Field.FieldType != typeof(Assembly))
            {
                Log($"Field {item.Field.Name} has wrong type. Expected Assembly, got {item.Field.FieldType}");
                continue;
            }
            
            Assembly foundAssembly = GetAssembly(item.Attribute.AssemblyName);
            item.Field.SetValue(null, foundAssembly);
        }
    }

    private static Assembly GetAssembly(string assemblyName)
    {
        var currentDomain = AppDomain.CurrentDomain;
        var assemblies = currentDomain.GetAssemblies();
            
        foreach (var assembly in assemblies)
        {
            if (assembly.GetName().Name == assemblyName)
            {
                Log("Found assembly: " + assemblyName);
                return assembly;
            }
        }
        Log("Couldn't find assembly: " + assemblyName);
        return null;
    }
}