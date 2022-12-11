// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.InteropServices;

namespace VRCOSC.Game.Processes;

internal static class ProcessKey
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    private const int keyeventf_keydown = 0x0000;
    private const int keyeventf_keyup = 0x0002;

    internal static void HoldKey(int key)
    {
        keybd_event((byte)key, (byte)key, keyeventf_keydown, 0);
    }

    internal static void ReleaseKey(int key)
    {
        keybd_event((byte)key, (byte)key, keyeventf_keyup, 0);
    }
}
