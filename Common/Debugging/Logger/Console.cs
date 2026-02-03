#if DEBUG
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Sims3.Gameplay.Core;
using Sims3.UI;
//DO NOT PUT NAMESPACE HERE
/// <summary>
/// Custom Console class for debug builds, directly invoking native console functions via unmanaged interop
/// </summary>
internal static class Console
{
    static Console()
    {
        if (!Commands.sGameCommands.mCommands.ContainsKey("ConsoleCreate"))
        {
            Commands.sGameCommands.Register("ConsoleCreate", "Manually creates the console window (if not already opened). Usage: ConsoleCreate", Commands.CommandType.General, ConsoleCheats.OnCreate, false);
            Commands.sGameCommands.Register("ConsoleClose", "Closes the console window and stops all logging. Usage: ConsoleClose", Commands.CommandType.General, ConsoleCheats.OnClose, false);
            Commands.sGameCommands.Register("ConsoleWriteLine", "Prints a message to the native console window. Usage: ConsoleWriteLine <message>", Commands.CommandType.General, ConsoleCheats.OnWriteLine, false);
            Commands.sGameCommands.Register("ConsoleClear", "Clears the console buffer and corresponding console window of display information. Usage: ConsoleClear", Commands.CommandType.General, ConsoleCheats.OnClear, false);
            Commands.sGameCommands.Register("ConsoleStartLogging", "Starts logging all console output to a file. Usage: ConsoleStartLogging <filename>", Commands.CommandType.General, ConsoleCheats.OnStartLogging, false);
            Commands.sGameCommands.Register("ConsoleStopLogging", "Stops logging to the specified log file. Usage: ConsoleStopLogging <filename>", Commands.CommandType.General, ConsoleCheats.OnStopLogging, false);
            Commands.sGameCommands.Register("ConsoleBeep", "Plays the sound of a beep through the console speaker. Usage: ConsoleBeep", Commands.CommandType.General, ConsoleCheats.OnBeep, false);
        }
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern bool _IsPresent();
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern void _Create();

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern void _Close();

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern unsafe void _WriteLine(sbyte* utf8Text);

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern unsafe void _StartLogging(sbyte* filenameUtf8);

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern unsafe void _StopLogging(sbyte* filenameUtf8);
	
	[MethodImpl(MethodImplOptions.InternalCall)]
    private static extern void _Clear();
	
	[MethodImpl(MethodImplOptions.InternalCall)]
    private static extern void _Beep();
    
    public static unsafe void WriteLine(string text)
    {
        if (!_IsPresent())
        {
            SimpleMessageDialog.Show("Sims3ConsolePlugin","[WriteLine] The Console is not present!");
            return;
        }

        using Utf8Ptr utf8Ptr = text;
        _WriteLine(utf8Ptr);
    }
    
    public static unsafe void StartLogging(string filename)
    {
        if (!_IsPresent())
        {
            SimpleMessageDialog.Show("Sims3ConsolePlugin","[StartLogging] The Console is not present!");
            return;
        }

        using Utf8Ptr utf8Ptr = filename;
        _StartLogging(utf8Ptr);
    }
    
    public static unsafe void StopLogging(string filename)
    {
        if (!_IsPresent())
        {
            SimpleMessageDialog.Show("Sims3ConsolePlugin","[StopLogging] The console is not present! Any logging done before the console was closed is already saved to a file.");
            return;
        }

        using Utf8Ptr utf8Ptr = filename;
        _StopLogging(utf8Ptr);
    }
    
    public static void Create()
    {
        if (_IsPresent())
        {
            SimpleMessageDialog.Show("Sims3ConsolePlugin","[Create] The Console is already present!");
        }
        else
        {
            _Create();
        }
            
    }
    public static void Close()
    {
        if (!_IsPresent())
        {
            SimpleMessageDialog.Show("Sims3ConsolePlugin","[Close] The Console is not present!");
        }
        else
        {
            _Close();
        }
            
    }
    public static void Clear()
    {
        if (!_IsPresent())
        {
            SimpleMessageDialog.Show("Sims3ConsolePlugin","[Clear] The Console is not present!");
        }
        else
        {
            _Clear();
        }
            
    }

    public static void Beep()
    {
        if (!_IsPresent())
        {
            SimpleMessageDialog.Show("Sims3ConsolePlugin","[Beep] The Console is not present!");
        }
        else
        {
            _Beep();
        }
    }

    public static class ConsoleCheats
    {
        public static int OnCreate(object[] parameters)
        {
            Create();
            return 1;
        }

        public static int OnClose(object[] parameters)
        {
            Close();
            return 1;
        }
        
        public static int OnWriteLine(object[] parameters)
        {
            if (parameters == null)
            {
                SimpleMessageDialog.Show("Sims3ConsolePlugin","[WriteLine] No parameters provided!");
            }
            if (parameters is { Length: > 0 })
            {
                StringBuilder sb = new StringBuilder();

                foreach (var t in parameters)
                {
                    if (t != null)
                    {
                        if (sb.Length > 0) sb.Append(" ");
                        sb.Append(t);
                    }
                }

                WriteLine(sb.ToString());
            }
            return 1;
        }
        
        public static int OnClear(object[] parameters)
        {
            Clear();
            return 1;
        }
        
        public static int OnStartLogging(object[] parameters)
        {
            if (parameters.Length == 0)
            {
                SimpleMessageDialog.Show("Sims3ConsolePlugin","[StartLogging] No filename provided!");
            }
            if (parameters.Length > 0 && parameters[0] != null)
            {
                StartLogging(parameters[0].ToString());
            }
            return 1;
        }

        public static int OnStopLogging(object[] parameters)
        {
            if (parameters.Length == 0)
            {
                SimpleMessageDialog.Show("Sims3ConsolePlugin","[StopLogging] No filename provided!");
            }
            if (parameters.Length > 0 && parameters[0] != null)
            {
                StopLogging(parameters[0].ToString());
            }
            return 1;
        }
        
        public static int OnBeep(object[] parameters)
        {
            Beep();
            return 1;
        }

    }
}

public unsafe struct Utf8Ptr : IDisposable
{
    private byte[] _bytes;
    private GCHandle _handle;

    public Utf8Ptr(string str)
    {
        _bytes = Encoding.UTF8.GetBytes(str + "\0");
        _handle = GCHandle.Alloc(_bytes, GCHandleType.Pinned);
        Pointer = (sbyte*)_handle.AddrOfPinnedObject().ToPointer();
    }

    public sbyte* Pointer { get; private set; }

    public void Dispose()
    {
        if (_handle.IsAllocated)
        {
            _handle.Free();
        }
        _bytes = null;
        Pointer = null;
    }

    public static implicit operator sbyte*(Utf8Ptr utf8Ptr)
    {
        return utf8Ptr.Pointer;
    }

    public static implicit operator Utf8Ptr(string str)
    {
        return new Utf8Ptr(str);
    }
}
#endif

#if !DEBUG
/// <summary>
/// Dummy class for release builds
/// </summary>
internal static class Console
{
    public static void Create() { /* Dummy - does nothing */ }
    
    public static void Close() { /* Dummy - does nothing */ }
    
    public static void WriteLine(string text) { /* Dummy - does nothing */ }
    
    public static void StartLogging(string filename) { /* Dummy - does nothing */ }
    
    public static void StopLogging(string filename) { /* Dummy - does nothing */ }
	
	public static void Clear() { /* Dummy - does nothing */ }
	
	public static void Beep() { /* Dummy - does nothing */ }

}
#endif