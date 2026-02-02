using System;
using System.Collections.Generic;
using System.Reflection;
using Sims3.SimIFace;

namespace Arro.Common;

internal static class AttributeCache
{
    private static readonly Dictionary<Type, List<MethodInfo>> _cachedMethods = new();
    private static readonly Dictionary<MethodInfo, object[]> _cachedMethodAttributes = new();
    private static readonly Dictionary<Type, List<FieldInfo>> _cachedFields = new();
    private static readonly Dictionary<FieldInfo, object[]> _cachedFieldAttributes = new();
    
    private static bool _initialized;
    
    public class MethodWithAttribute<T> where T : Attribute
    {
        public MethodInfo Method { get; set; }
        public T Attribute { get; set; }
    }

    public class FieldWithAttribute<T> where T : Attribute
    {
        public FieldInfo Field { get; set; }
        public T Attribute { get; set; }
    }

    private static Assembly assembly;
    private static float elapsedTime;
    public static void Initialize()
    {
        if (_initialized) return;
        assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();
        var stopwatch = StopWatch.Create(StopWatch.TickStyles.Milliseconds);
        foreach (var type in types)
        {
            CacheMethods(type);
            CacheFields(type);
        }
        stopwatch.Stop();
        elapsedTime = stopwatch.GetElapsedTimeFloat();
        _initialized = true;
    }

    private static void CacheMethods(Type type)
    {
        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | 
                                      BindingFlags.NonPublic);
    
        foreach (var method in methods)
        {
            var attributes = method.GetCustomAttributes(false);
            if (attributes.Length == 0) continue;
            
            var relevantAttributes = new List<object>();
            foreach (var attr in attributes)
            {
                var attrType = attr.GetType();
                
                if (attrType.Namespace != null && attrType.Namespace.StartsWith("Arro"))
                {
                    relevantAttributes.Add(attr);
                }
            }
        
            if (relevantAttributes.Count == 0) continue;
        
            _cachedMethodAttributes[method] = relevantAttributes.ToArray();
        
            foreach (var attr in relevantAttributes)
            {
                var attrType = attr.GetType();
                if (!_cachedMethods.ContainsKey(attrType))
                {
                    _cachedMethods[attrType] = new List<MethodInfo>();
                }
            
                if (!_cachedMethods[attrType].Contains(method))
                {
                    _cachedMethods[attrType].Add(method);
                }
            }
        }
    }

    private static void CacheFields(Type type)
    {
        var fields = type.GetFields(BindingFlags.Static | 
                                    BindingFlags.Public | BindingFlags.NonPublic);
    
        foreach (var field in fields)
        {
            var attributes = field.GetCustomAttributes(false);
            if (attributes.Length == 0) continue;
            
            var relevantAttributes = new List<object>();
            foreach (var attr in attributes)
            {
                var attrType = attr.GetType();
                
                if (attrType.Namespace != null && attrType.Namespace.StartsWith("Arro"))
                {
                    relevantAttributes.Add(attr);
                }
            }
        
            if (relevantAttributes.Count == 0) continue;
            
            _cachedFieldAttributes[field] = relevantAttributes.ToArray();
            
            foreach (var attr in relevantAttributes)
            {
                var attrType = attr.GetType();
                if (!_cachedFields.ContainsKey(attrType))
                {
                    _cachedFields[attrType] = new List<FieldInfo>();
                }
            
                if (!_cachedFields[attrType].Contains(field))
                {
                    _cachedFields[attrType].Add(field);
                }
            }
        }
    }
    
    public static List<MethodInfo> GetMethodsWithAttribute<T>() where T : Attribute
    {
        return _cachedMethods.TryGetValue(typeof(T), out var methods) 
            ? methods 
            : new List<MethodInfo>();
    }
    
    public static List<FieldInfo> GetFieldsWithAttribute<T>() where T : Attribute
    {
        return _cachedFields.TryGetValue(typeof(T), out var fields) 
            ? fields 
            : new List<FieldInfo>();
    }
    
    public static List<MethodWithAttribute<T>> GetMethodsWithAttributeEx<T>() where T : Attribute
    {
        var result = new List<MethodWithAttribute<T>>();
        
        if (!_cachedMethods.TryGetValue(typeof(T), out var methods))
            return result;
        
        foreach (var method in methods)
        {
            if (!_cachedMethodAttributes.TryGetValue(method, out var allAttributes))
                continue;
            
            foreach (var attr in allAttributes)
            {
                if (attr is T typedAttr)
                {
                    result.Add(new MethodWithAttribute<T>
                    {
                        Method = method,
                        Attribute = typedAttr
                    });
                }
            }
        }
        
        return result;
    }
    
    public static List<FieldWithAttribute<T>> GetFieldsWithAttributeEx<T>() where T : Attribute
    {
        var result = new List<FieldWithAttribute<T>>();
        
        if (!_cachedFields.TryGetValue(typeof(T), out var fields))
            return result;
        
        foreach (var field in fields)
        {
            if (!_cachedFieldAttributes.TryGetValue(field, out var allAttributes))
                continue;
            
            foreach (var attr in allAttributes)
            {
                if (attr is T typedAttr)
                {
                    result.Add(new FieldWithAttribute<T>
                    {
                        Field = field,
                        Attribute = typedAttr
                    });
                }
            }
        }
        
        return result;
    }
    
    public static object[] GetAttributesForMethod(MethodInfo method)
    {
        return _cachedMethodAttributes.TryGetValue(method, out var attrs) 
            ? attrs 
            : [];
    }
    
    public static object[] GetAttributesForField(FieldInfo field)
    {
        return _cachedFieldAttributes.TryGetValue(field, out var attrs) 
            ? attrs 
            : [];
    }
    
    public static void Clear()
    {
        _cachedMethods.Clear();
        _cachedMethodAttributes.Clear();
        _cachedFields.Clear();
        _cachedFieldAttributes.Clear();
        _initialized = false;
    }
    
    public static void PrintStats()
    {
        Logger.Log("=== Attribute Cache ===");
        Logger.Log($"Took {elapsedTime}ms");
        Logger.Log($"Total methods with attributes: {_cachedMethodAttributes.Count}");
        Logger.Log($"Total fields with attributes: {_cachedFieldAttributes.Count}");
        
        if (_cachedMethodAttributes.Count > 0)
        {
            Logger.Log("\n[Methods with attributes]:");
            foreach (var kvp in _cachedMethodAttributes)
            {
                var method = kvp.Key;
                var attrs = kvp.Value;
                Logger.Log($"  {method.DeclaringType?.Name}.{method.Name}");
                foreach (var attr in attrs)
                {
                    Logger.Log($"    - {attr.GetType().Name}");
                }
            }
        }
        
        if (_cachedFieldAttributes.Count > 0)
        {
            Logger.Log("\n[Fields with attributes]:");
            foreach (var kvp in _cachedFieldAttributes)
            {
                var field = kvp.Key;
                var attrs = kvp.Value;
                Logger.Log($"  {field.DeclaringType?.Name}.{field.Name}");
                foreach (var attr in attrs)
                {
                    Logger.Log($"    - {attr.GetType().Name}");
                }
            }
        }
    }
}