using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Sims3.Gameplay;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using OneShotFunctionTask = Sims3.Gameplay.OneShotFunctionTask;

namespace Arro.Common.Debugging;

public static class ExceptionCommon
{
    public static void Handle(
        Exception e,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
#if DEBUG
        Console.Beep();
        var fileName = filePath.Split('\\', '/').Last();
        Logger.Log($"Exception in {memberName} ({fileName}:line {lineNumber}): {e}");
#else
        new ExceptionHandler(memberName, filePath, lineNumber, e).Route();
#endif
    }
}

public class ExceptionHandler(string memberName, string filePath, int lineNumber, Exception exception)
{
    public void Route()
    {
        if (LoadingScreenController.Instance != null)
        {
            Simulator.AddObject(new OneShotFunctionTask(WaitForLoadingScreen));
            return;
        }
        
        if (GameStates.IsInWorld())
        {
            ShowButtonNotification();
            return;
        }
        var content = FormatException(exception, memberName, filePath, lineNumber);
        WriteErrorXMLFile(memberName + "_error", content);

        var fileName = filePath.Split('\\', '/').Last();
        SimpleMessageDialog.Show(
            "Exception",
            $"{exception.GetType().Name}: {exception.Message}\n" +
            $"in {memberName} ({fileName}:line {lineNumber})\n" +
            $"Error details saved to The Sims 3 folder."
        );
    }
    
    private void WaitForLoadingScreen()
    {
        if (LoadingScreenController.Instance != null)
        {
            Simulator.AddObject(new OneShotFunctionTask(WaitForLoadingScreen, StopWatch.TickStyles.Seconds, 1f));
            return;
        }
        Route();
    }
    
    public void ShowButtonNotification()
    {
        var fileName = filePath.Split('\\', '/').Last();
        string title = $"Exception {exception.GetType().Name}: {exception.Message}\n" +
                       $"in {memberName} ({fileName}:line {lineNumber})\n" +
                       $"Click to save full details.";

        StyledNotification.Format format = new StyledNotification.Format(
            title,
            "Save info",
            ButtonCallback,
            StyledNotification.NotificationStyle.kSystemMessage
        )
        {
            mCloseOnCallback = true
        };

        StyledNotification.Show(format, "placeholder");
    }

    private void ButtonCallback()
    {
        WriteErrorXMLFile(memberName + "_error", FormatException(exception, memberName, filePath, lineNumber));
    }

    public static string FormatException(Exception e, string memberName, string filePath, int lineNumber)
    {
        var fileName = filePath.Split('\\', '/').Last();
        var sb = new StringBuilder();
        
        sb.AppendLine($"Location: {memberName} in {fileName}:line {lineNumber}");
        sb.AppendLine();

        var current = e;
        int depth = 0;
        while (current != null)
        {
            if (depth > 0)
                sb.AppendLine($"--- Inner Exception ---");

            sb.AppendLine($"{current.GetType().FullName}: {current.Message}");

            if (current.StackTrace != null)
                sb.AppendLine(current.StackTrace);

            current = current.InnerException;
            depth++;
        }
        return sb.ToString();
    }

    public static void WriteErrorXMLFile(string fileName, string content)
    {
        uint num = 0u;
        Simulator.CreateExportFile(ref num, fileName);

        if (num != 0)
        {
            CustomXmlWriter customXmlWriter = new CustomXmlWriter(num);
            customXmlWriter.WriteToBuffer(content);
            customXmlWriter.WriteEndDocument();
        }
    }
}