using System;
using System.Collections.Generic;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.UI;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.SimIFace.CustomContent;
using Sims3.UI.CAS;

namespace Arro.Common;

internal static class IniParser
{
    internal static string Serialize<T>(T obj)
    {
        var lines = new List<string>();
        foreach (var prop in typeof(T).GetFields())
        {
            lines.Add($"{prop.Name}={prop.GetValue(obj)}");
        }
        return string.Join(";", lines.ToArray());
    }

    public static T Deserialize<T>(string data) where T : class, new()
    {
        T obj = new T();
        if (string.IsNullOrEmpty(data)) return obj;

        string[] pairs = data.Split(';');
        foreach (string pair in pairs)
        {
            string[] kv = pair.Split('=');
            if (kv.Length != 2) continue;

            var prop = typeof(T).GetField(kv[0]);
            if (prop != null)
            {
                object value = Convert.ChangeType(kv[1], prop.FieldType);
                prop.SetValue(obj, value);
            }
        }
        return obj;
    }
}
public abstract class IniConfig
{
    /// <summary>
    /// Handles mod configuration by hijacking SimDescription objects and storing data in their Bio field.
    /// This allows persistent storage within the game's SavedSims folder without external file IO.
    /// </summary>
    public static void Save<T>(string fileName, T config) where T : class
    {
        CASLogic singleton = CASLogic.GetSingleton();
        if (singleton == null || singleton.SimDescriptions.Count == 0) return;

        string[] nameParts = fileName.Split('_');
        string firstName = nameParts[0];
        string lastName = nameParts.Length > 1 ? nameParts[1] : "Config";
        
        string safeData = IniParser.Serialize(config);
        
        for (int i = 0; i < singleton.SimDescriptions.Count; i++)
        {
            ResourceKey key = singleton.SimDescriptions[i];
            ResourceKeyContentCategory cat = ResourceKeyContentCategory.kInstalled;
            ISimDescription existing = singleton.GetSimDescription(key, ref cat);

            if (existing != null && existing.FirstName == firstName && existing.LastName == lastName)
            {
                if (Responder.Instance?.CASModel != null)
                    Responder.Instance.CASModel.DeleteSimFromContent(key);
            }
        }
        
        ResourceKey templateKey = singleton.SimDescriptions[5];
        ResourceKeyContentCategory tCat = ResourceKeyContentCategory.kInstalled;
        SimDescription templateSim = new SimDescription();
        SimDescription baseSim = singleton.GetSimDescription(templateKey, ref tCat) as SimDescription;
        ResourceKey skipOutfitKey = new ResourceKey(
            0x0000000000000001,  // Instance (non-zero to avoid null checks)
            0x025ed6f4,          // The special type that triggers alternate path
            0x00000001           // Base game group
        );
        if (templateSim != null)
        {
            templateSim.Fixup(); //Crucial
            templateSim.FirstName = firstName;
            templateSim.LastName = lastName;
            templateSim.Bio = safeData;
            templateSim.AgeGenderSpecies = CASAgeGenderFlags.None;
            templateSim.Outfits = baseSim.Outfits;
            try {
                DownloadContent.SaveCustomSim((IExportableContent) templateSim, templateSim.FirstName + "_" + templateSim.LastName,
                   skipOutfitKey,
                    0, 0u);
                
                Logger.Log($"Config saved as INI Sim: {firstName}_{lastName} outfit : {templateSim.GetOutfit(OutfitCategories.Everyday, 0).Key}");
            } catch (Exception e) {
                Logger.Log("Crash prevented during SaveSim: " + e.Message);
            }
        }
    }

    /// <summary>
    /// Loads the configuration by searching for a local Sim where FirstName and LastName match the provided fileName.
    /// </summary>
    /// <typeparam name="T">The type of the config object to return.</typeparam>
    /// <param name="fileName">The identifier (format: "FirstName_LastName") to search for.</param>
    /// <returns>A populated config object of type T, or a new instance if not found.</returns>
    public static T Load<T>(string fileName) where T : class, new()
    {
        CASLogic singleton = CASLogic.GetSingleton();
        if (singleton == null) return new T();
        
        string[] nameParts = fileName.Split('_');
        string searchFirst = nameParts[0];
        string searchLast = nameParts.Length > 1 ? nameParts[1] : "Config";

        for (int i = 0; i < singleton.SimDescriptions.Count; i++)
        {
            ResourceKey key = singleton.SimDescriptions[i];
            ResourceKeyContentCategory cat = ResourceKeyContentCategory.kInstalled;
            ISimDescription sd = singleton.GetSimDescription(key, ref cat);
            
            if (sd != null && cat == ResourceKeyContentCategory.kLocalUserCreated)
            {
                if (sd.FirstName == searchFirst && sd.LastName == searchLast)
                {
                    Logger.Log($"Found config Sim: {searchFirst}_{searchLast}");
                    return IniParser.Deserialize<T>(sd.Bio);
                }
            }
        }
        
        Logger.Log($"Config {fileName} not found, returning defaults.");
        return new T();
    }
}