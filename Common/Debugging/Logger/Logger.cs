namespace Arro.Common;

internal abstract class Logger
{
    /// <summary>
    /// Writes a message to the console prefixed with the mod name.
    /// </summary>
    /// <param name="message">The object or string to log.</param>
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
    public static void Assert(bool condition, string message = null)
    {
        if (!condition)
        {
            var stackTrace = new System.Diagnostics.StackTrace(true);
            var callerFrame = stackTrace.GetFrame(1);
            var method = callerFrame.GetMethod();
            var methodName = method.Name;
            var className = method.DeclaringType?.FullName ?? "Unknown";
            var errorMessage = message ?? "Assertion failed";
        
            Console.WriteLine($"[{Core.modName}] {errorMessage}");
            Console.WriteLine($"  at {className}.{methodName}");
        }
    }
}