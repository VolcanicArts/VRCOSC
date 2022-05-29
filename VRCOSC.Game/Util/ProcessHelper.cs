// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Linq;

namespace VRCOSC.Game.Util;

public enum ShowWindowEnum
{
    Hide = 0,
    ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
    Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
    Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
    Restore = 9, ShowDefault = 10, ForceMinimized = 11
};

public static class ProcessHelper
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int SetForegroundWindow(IntPtr hwnd);

    public static void ShowMainWindow(Process handle, ShowWindowEnum showWindowEnum)
    {
        ShowWindow(handle.MainWindowHandle, showWindowEnum);
    }

    public static void ShowMainWindow(string processName, ShowWindowEnum showWindowEnum)
    {
        Process? bProcess = Process.GetProcessesByName(processName).FirstOrDefault();
        if (bProcess != null) ShowMainWindow(bProcess, showWindowEnum);
    }

    public static void SetMainWindowForeground(Process handle)
    {
        SetForegroundWindow(handle.MainWindowHandle);
    }

    public static void SetMainWindowForeground(string processName)
    {
        Process? bProcess = Process.GetProcessesByName(processName).FirstOrDefault();
        if (bProcess != null) SetMainWindowForeground(bProcess);
    }
}
