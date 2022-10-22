// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VRCOSC.Game.Util;

internal static class ProcessWindow
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

    [DllImport("user32.dll")]
    private static extern int SetForegroundWindow(IntPtr hwnd);

    internal static void ShowMainWindow(Process handle, ShowWindowEnum showWindowEnum)
    {
        ShowWindow(handle.MainWindowHandle, showWindowEnum);
    }

    internal static void SetMainWindowForeground(Process handle)
    {
        SetForegroundWindow(handle.MainWindowHandle);
    }
}

public enum ShowWindowEnum
{
    Hide = 0,
    ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
    Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
    Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
    Restore = 9, ShowDefault = 10, ForceMinimized = 11
};
