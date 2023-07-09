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

    public static void ShowMainWindow(this Process process, User32.WindowShowStyle style) => User32.ShowWindow(process.MainWindowHandle, style);
    public static void SetMainWindowForeground(this Process process) => User32.SetForegroundWindow(process.MainWindowHandle);

    public static float RetrieveProcessVolume(string? processName)
    {
        if (processName is null) return 1f;

        MMDeviceEnumerator deviceiterator = new MMDeviceEnumerator();
        var speakers = deviceiterator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        var count = speakers.AudioSessionManager.Sessions.Count;

        for (var i = 0; i < count; i++)
        {
            var session = speakers.AudioSessionManager.Sessions[i];

            if (session.GetSessionIdentifier.Contains(processName, StringComparison.InvariantCultureIgnoreCase))
            {
                return session.SimpleAudioVolume.Volume;
            }
        }

        return 1f;
    }

    public static void SetProcessVolume(string? processName, float percentage)
    {
        if (processName is null) return;

        MMDeviceEnumerator deviceiterator = new MMDeviceEnumerator();
        var speakers = deviceiterator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        var count = speakers.AudioSessionManager.Sessions.Count;

        for (var i = 0; i < count; i++)
        {
            var session = speakers.AudioSessionManager.Sessions[i];

            if (session.GetSessionIdentifier.Contains(processName, StringComparison.InvariantCultureIgnoreCase))
            {
                session.SimpleAudioVolume.Volume = percentage;
            }
        }
    }
}
