using System;
using System.Collections.Generic;
#if DEBUG
using Sims3.Gameplay;
using Sims3.Gameplay.Core;
using Sims3.SimIFace;
using Arro.Common;
using Sims3.UI;

internal static class ConsoleInput
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
        
        if (!Commands.sGameCommands.mCommands.ContainsKey("ConsoleCreate")) //Register once per game
        {
            OnConsoleInput += InternalOnConsoleInput;
        }
        
    }
    
    private static void OnTick(WindowBase sender, UIEventArgs eventArgs)
    {
        PollInput();
    }

    private static void InternalOnConsoleInput(string value)
    {
        if (value == "quit")
        {
            GameStates.TransitionToGameStateQuitNoCheck();
        }
        if (value == "mainmenu")
        {
            GameStates.TransitionToLeaveInWorld();
        }
        if (value.StartsWith("command "))
        {
            string command = value.Substring("command ".Length);

            bool success = CommandSystem.ExecuteCommandString(command);

            Console.WriteLine(success
                ? $"Command: {command}"
                : $"Failed to execute: {command}");
        }
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