using osu.Framework.Logging;

namespace VRCOSC.Game.Util;

public class TerminalLogger
{

    private string ModuleName;

    public TerminalLogger(string moduleName)
    {
        ModuleName = moduleName;
    }

    public void Log(string message)
    {
        Logger.Log($"[{ModuleName}]: {message}", "terminal");
    }

    public static void Log(string message, string moduleName)
    {
        Logger.Log($"[{moduleName}]: {message}", "terminal");
    }
}
