// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
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

    public static string? GetActiveWindowTitle()
    {
        var foregroundWindowHandle = User32.GetForegroundWindow();
        if (foregroundWindowHandle == IntPtr.Zero) return null;

        User32.GetWindowThreadProcessId(foregroundWindowHandle, out int processId);

        if (processId <= 0) return null;

        try
        {
            return Process.GetProcessById(processId).ProcessName;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public static void ShowMainWindow(this Process process, User32.WindowShowStyle style) => User32.ShowWindow(process.MainWindowHandle, style);
    public static void SetMainWindowForeground(this Process process) => User32.SetForegroundWindow(process.MainWindowHandle);

    private static SimpleAudioVolume? getProcessAudioVolume(string? processName)
    {
        if (processName is null) return null;

        MMDeviceEnumerator deviceiterator = new MMDeviceEnumerator();
        var speakers = deviceiterator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        var count = speakers.AudioSessionManager.Sessions.Count;

        for (var i = 0; i < count; i++)
        {
            var session = speakers.AudioSessionManager.Sessions[i];

            if (session.GetSessionIdentifier.Contains(processName, StringComparison.InvariantCultureIgnoreCase))
            {
                return session.SimpleAudioVolume;
            }
        }
    }

    public static float RetrieveProcessVolume(string? processName) => getProcessAudioVolume(processName)?.Volume ?? 1f;

    public static void SetProcessVolume(string? processName, float percentage)
    {
        var processAudioVolume = getProcessAudioVolume(processName);
        if (processAudioVolume is null) return;

        processAudioVolume.Volume = percentage;
    }
}
