namespace Arro.Common;

internal static class Core
{
    public static string modName  {get; set;}

    /// <summary>
    /// Orchestrates the startup sequence of the mod by setting the mod identity 
    /// and initializing core reflection and event-handling systems.
    /// </summary>
    /// <param name="name">The display name of the mod, used for logging and console prefixes.</param>
    public static void Initialize(string name)
    {
        modName = name;
        AttributeCache.Initialize();
        InvokeOnEvent.Initialize();
    }

    [InvokeOnWorldEvent(Event.OnStartupApp)]
    public static void OnStartupApp()
    {
        AttributeCache.PrintStats();
        GameCommand.Initialize();
        AssemblyChecker.Initialize();
    }
}