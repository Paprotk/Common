using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Arro.Common;

internal abstract class Logger
{
    /// <summary>
    /// Writes a message to the console prefixed with the mod name.
    /// </summary>
    /// <param name="message">The object or string to log.</param>
    [Conditional("DEBUG")]
    public static void Log(object message)
    {
        Console.WriteLine($"[{Core.modName}] {message}");
    }
    
    /// <summary>
    /// Checks a condition; if false, logs an error message along with the 
    /// class and method name of the caller.
    /// </summary>
    /// <param name="condition">The condition to verify.</param>
    /// <param name="message">Optional custom error message.</param>
    [Conditional("DEBUG")]
    public static void Assert(
        bool condition, 
        string message = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!condition)
        {
            var errorMessage = message ?? "Assertion failed";
            
            string fileName = filePath.Split('\\', '/').Last();

            Console.WriteLine($"[{Core.modName}] {errorMessage}");
            Console.WriteLine($"  at {memberName} in {fileName}:line {lineNumber}");
        }
    }
}