using System;
using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.UI;
using Sims3.SimIFace;
using Sims3.SimIFace.CustomContent;
using System.Reflection;

namespace Arro.Common;

/// <summary>
/// Provides a lightweight reflection-based engine for serializing and deserializing 
/// simple objects into a semicolon-delimited string format.
/// </summary>
internal static class IniParser
{
    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance;

    internal static string Serialize<T>(T obj)
    {
        var lines = new List<string>();
        var type = typeof(T);

        foreach (var member in type.GetMembers(Flags))
        {
            object val = null;
            if (member is FieldInfo f) val = f.GetValue(obj);
            else if (member is PropertyInfo p && p.CanRead) val = p.GetValue(obj, null);

            if (val != null)
            {
                // Escape: | -> ||, = -> |e, ; -> |s
                string safeValue = val.ToString()
                    .Replace("|", "||")
                    .Replace("=", "|e")
                    .Replace(";", "|s");
                lines.Add(member.Name + "=" + safeValue);
            }
        }
        return string.Join(";", lines.ToArray());
    }

    public static T Deserialize<T>(string data) where T : class, new()
    {
        var obj = new T();
        if (string.IsNullOrEmpty(data)) return obj;

        foreach (var pair in data.Split(';'))
        {
            int idx = pair.IndexOf('=');
            if (idx <= 0) continue;

            string name = pair.Substring(0, idx);
            // Unescape: |s -> ;, |e -> =, || -> |
            string rawValue = pair.Substring(idx + 1)
                .Replace("|s", ";")
                .Replace("|e", "=")
                .Replace("||", "|");

            var type = typeof(T);
            var field = type.GetField(name, Flags);
            if (field != null) {
                field.SetValue(obj, Convert.ChangeType(rawValue, field.FieldType));
                continue;
            }
            var prop = type.GetProperty(name, Flags);
            if (prop != null && prop.CanWrite) {
                prop.SetValue(obj, Convert.ChangeType(rawValue, prop.PropertyType), null);
            }
        }
        return obj;
    }
}

public abstract class IniConfig
{
    /// <summary>
    /// Persists the configuration object by hijacking a Sim container and saving it to the user's library.
    /// </summary>
    /// <typeparam name="T">The reference type containing configuration data.</typeparam>
    /// <param name="fileName">The unique identifier (mapped to Sim's FirstName) used as the filename on disk.</param>
    /// <param name="config">The data object to be serialized and stored.</param>
    /// <remarks>
    /// This method serializes the object into a string and embeds it into the Bio field of a dummy Sim. 
    /// It performs an "upsert" by scanning the SavedSims folder for any existing Sim with a FirstName 
    /// matching <paramref name="fileName"/> and deleting it before writing the new data.
    /// This method is quite resource intensive as it relies on P/Invoke so use it when necessary.
    /// </remarks>
    public static void Save<T>(string fileName, T config) where T : class
    {
        var startTime = DateTime.Now;
        Logger.Assert(GameUtils.IsValidFilename(fileName));
        if (!GameUtils.IsValidFilename(fileName)) return; //fileName cannot contain illegal characters
        var singleton = CASLogic.GetSingleton(); //Works only when world is loaded
        if (singleton == null || singleton.SimDescriptions.Count == 0) return;
        
        var serializedData = IniParser.Serialize(config);
        
        for (var i = 0; i < singleton.SimDescriptions.Count; i++)
        {
            var key = singleton.SimDescriptions[i];
            var category = ResourceKeyContentCategory.kLocalUserCreated;
            var simDescription = singleton.GetSimDescription(key, ref category);
            
            if (simDescription == null || category != ResourceKeyContentCategory.kLocalUserCreated) continue;

            if (simDescription.FirstName != fileName) continue;
            if (Responder.Instance?.CASModel != null)
                Responder.Instance.CASModel.DeleteSimFromContent(key);
            break;
        }
        var iniSim = new SimDescription();
        iniSim.Fixup(); //Essential!
        iniSim.FirstName = fileName;
        iniSim.Bio = serializedData;
        try
        {
            DownloadContent.SaveCustomSim(iniSim, fileName,
                new ResourceKey(0x0000000000000000, 0x025ed6f4, 0x00000000), //Generate Empty thumbnail
                0, 0u); //Setting ageGenderSpecies to 0u will hide this sim from CAS bin UI
        }
        catch (Exception e)
        {
            Logger.Log(e);
        }
        var endTime = DateTime.Now;
        var duration = endTime - startTime;
        Logger.Log($"Saved config: {fileName}, took {duration.TotalSeconds:F3}s");
        iniSim.Dispose();
    }

    /// <summary>
    /// Loads configuration data from a .sim file located in the user's SavedSims directory.
    /// </summary>
    /// <typeparam name="T">The type of configuration object to return.</typeparam>
    /// <param name="fileName">The identifier (matching the Sim's FirstName) used to locate the data.</param>
    /// <returns>
    /// A populated instance of <typeparamref name="T"/> if found and successfully parsed; 
    /// otherwise, a new instance of <typeparamref name="T"/> with default values.
    /// </returns>
    /// <remarks>
    /// This method scans the local library for a dummy Sim whose FirstName matches the <paramref name="fileName"/>.
    /// It then extracts and deserializes the configuration stored within that Sim's Bio field.
    /// </remarks>
    public static T Load<T>(string fileName) where T : class, new()
    {
        var startTime = DateTime.Now;
        Logger.Assert(GameUtils.IsValidFilename(fileName));
        if (!GameUtils.IsValidFilename(fileName)) return new T(); //fileName cannot contain illegal characters
        var singleton = CASLogic.GetSingleton(); //Works only when world is loaded
        if (singleton == null) return new T();

        for (var i = 0; i < singleton.SimDescriptions.Count; i++)
        {
            var key = singleton.SimDescriptions[i];
            var category = ResourceKeyContentCategory.kLocalUserCreated;
            var simDescription = singleton.GetSimDescription(key, ref category);

            if (simDescription == null || category != ResourceKeyContentCategory.kLocalUserCreated) continue;
            if (simDescription.FirstName != fileName) continue;
            if (string.IsNullOrEmpty(simDescription.Bio)) continue;
            try
            {
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                Logger.Log($"Loading config: {fileName}, took {duration.TotalSeconds:F3}s");
                return IniParser.Deserialize<T>(simDescription.Bio);
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }
        
        Logger.Log($"Config {fileName} not found, returning defaults.");
        return new T();
    }
}