// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VRCOSC.Game.Processes;

internal static class ProcessWindow
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

    [DllImport("user32.dll")]
    private static extern int SetForegroundWindow(IntPtr windowIntPtr);

    internal static void ShowMainWindow(Process handle, ShowWindowEnum showWindowEnum)
    {
        ShowWindow(handle.MainWindowHandle, showWindowEnum);
    }

    internal static void SetMainWindowForeground(Process handle)
    {
        _ = SetForegroundWindow(handle.MainWindowHandle);
    }
}

public enum ShowWindowEnum
{
    Hide,
    ShowNormal,
    ShowMinimized,
    ShowMaximized,
    ShowNormalNoActivate,
    Show,
    Minimize,
    ShowMinNoActivate,
    ShowNoActivate,
    Restore,
    ShowDefault,
    ForceMinimized
}
