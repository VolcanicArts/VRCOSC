// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
    [DllImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

    [DllImport("user32.dll")]
    private static extern int SetForegroundWindow(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    private const int keyeventf_keyup = 0x0002;

    public static void ShowMainWindow(Process handle, ShowWindowEnum showWindowEnum)
    {
        ShowWindow(handle.MainWindowHandle, showWindowEnum);
    }

    public static void SetMainWindowForeground(Process handle)
    {
        SetForegroundWindow(handle.MainWindowHandle);
    }

    public static void HoldKey(int key)
    {
        keybd_event((byte)key, (byte)key, 0, 0);
    }

    public static void ReleaseKey(int key)
    {
        keybd_event((byte)key, (byte)key, keyeventf_keyup, 0);
    }
}
