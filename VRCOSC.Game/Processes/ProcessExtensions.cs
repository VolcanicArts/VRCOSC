// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Extensions.IEnumerableExtensions;
using PInvoke;

namespace VRCOSC.Game.Processes;

public static class ProcessExtensions
{
    public static async Task PressKeys(IEnumerable<User32.VirtualKey> keys, int pressTime)
    {
        var windowsVKeys = keys as User32.VirtualKey[] ?? keys.ToArray();
        windowsVKeys.ForEach(key => User32.keybd_event((byte)(int)key, (byte)(int)key, 0, IntPtr.Zero));
        await Task.Delay(pressTime);
        windowsVKeys.ForEach(key => User32.keybd_event((byte)(int)key, (byte)(int)key, User32.KEYEVENTF.KEYEVENTF_KEYUP, IntPtr.Zero));
    }

    public static void ShowMainWindow(this Process process, User32.WindowShowStyle style) => User32.ShowWindow(process.MainWindowHandle, style);
    public static void SetMainWindowForeground(this Process process) => User32.SetForegroundWindow(process.MainWindowHandle);

    public static float RetrieveProcessVolume(string? processName)
    {
        if (processName is null) return 1f;

        return ProcessVolume.GetApplicationVolume(processName) ?? 1f;
    }

    public static bool IsProcessMuted(string? processName)
    {
        if (processName is null) return false;

        return ProcessVolume.GetApplicationMute(processName) ?? false;
    }

    public static void SetProcessVolume(string? processName, float percentage)
    {
        if (processName is null) return;

        ProcessVolume.SetApplicationVolume(processName, percentage);
    }

    public static void SetProcessMuted(string? processName, bool muted)
    {
        if (processName is null) return;

        ProcessVolume.SetApplicationMute(processName, muted);
    }
}
