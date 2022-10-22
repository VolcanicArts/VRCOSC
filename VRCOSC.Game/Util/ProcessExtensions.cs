// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Util;

public static class ProcessExtensions
{
    #region Keys

    public static async Task PressKeys(IEnumerable<WindowsVKey> keys, int pressTime)
    {
        var windowsVKeys = keys as WindowsVKey[] ?? keys.ToArray();
        windowsVKeys.ForEach(key => ProcessKey.HoldKey((int)key));
        await Task.Delay(pressTime);
        windowsVKeys.ForEach(key => ProcessKey.ReleaseKey((int)key));
    }

    public static async Task PressKey(WindowsVKey key, int pressTime)
    {
        ProcessKey.HoldKey((int)key);
        await Task.Delay(pressTime);
        ProcessKey.ReleaseKey((int)key);
    }

    #endregion

    #region Window

    public static void ShowMainWindow(Process process, ShowWindowEnum showWindowEnum)
    {
        ProcessWindow.ShowMainWindow(process, showWindowEnum);
    }

    public static void SetMainWindowForeground(Process process)
    {
        ProcessWindow.SetMainWindowForeground(process);
    }

    #endregion

    #region Volume

    public static float RetrieveProcessVolume(int pid)
    {
        return ProcessVolume.GetApplicationVolume(pid) ?? 0f;
    }

    public static bool IsProcessMuted(int pid)
    {
        return ProcessVolume.GetApplicationMute(pid) ?? false;
    }

    public static void SetProcessVolume(int pid, float percentage)
    {
        ProcessVolume.SetApplicationVolume(pid, percentage);
    }

    public static void SetProcessMuted(int pid, bool muted)
    {
        ProcessVolume.SetApplicationMute(pid, muted);
    }

    #endregion
}
