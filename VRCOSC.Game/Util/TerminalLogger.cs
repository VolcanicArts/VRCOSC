// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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
