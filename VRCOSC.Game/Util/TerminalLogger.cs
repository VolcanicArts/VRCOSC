// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Logging;

namespace VRCOSC.Game.Util;

public class TerminalLogger
{
    private readonly string moduleName;

    public TerminalLogger(string moduleName)
    {
        this.moduleName = moduleName;
    }

    public void Log(string message)
    {
        Log(message, moduleName);
    }

    public static void Log(string message, string moduleName)
    {
        Logger.Log($"[{moduleName}]: {message}", "terminal");
    }
}
