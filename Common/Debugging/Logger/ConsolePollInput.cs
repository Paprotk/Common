using System;
#if DEBUG
using Arro.Common;
using Sims3.UI;

internal static class ConsolePollInput
{
    /// <summary>
    /// Fired every tick a non-empty line arrives from the console.
    /// Subscribers receive the trimmed, lowercase command string.
    /// </summary>
    public static event Action<string> OnConsoleInput;

    [InvokeOnWorldEvent(Event.OnStartupApp)]
    internal static void OnStartupApp()
    {
        var gameMainWindow = UIManager.GetMainWindow();
        gameMainWindow.Tick += OnTick;
    }

    private static void OnTick(WindowBase sender, UIEventArgs eventArgs)
    {
        PollInput();
    }

    public static void PollInput()
    {
        string cmd = Console.PollInput();

        if (string.IsNullOrEmpty(cmd))
            return;

        cmd = cmd.Trim();

        OnConsoleInput?.Invoke(cmd);
    }
}
#endif
#if !DEBUG
internal static class ConsolePollInput
{
    public static event Action<string> OnConsoleInput { add { } remove { } }
    public static void PollInput() { }
}
#endif