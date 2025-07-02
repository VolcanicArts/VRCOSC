// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace VRCOSC.App.Utils;

internal class GlobalKeyboardHook : IDisposable
{
    private readonly HOOKPROC hookProc;
    private HHOOK hookId;

    private readonly ConcurrentDictionary<Key, bool> keyStates = [];
    public bool IsEnabled { get; private set; }

    public GlobalKeyboardHook()
    {
        hookProc = hookCallback;
        hookId = new HHOOK(IntPtr.Zero);
    }

    public bool GetKeyState(Key key) => keyStates.GetValueOrDefault(key, false);

    public unsafe void Enable()
    {
        if (hookId.Value != (void*)IntPtr.Zero)
            return;

        hookId = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, hookProc, HINSTANCE.Null, 0);

        if (hookId.Value == (void*)IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to set global keyboard hook");

        IsEnabled = true;
    }

    public unsafe void Disable()
    {
        if (hookId.Value != (void*)IntPtr.Zero)
        {
            PInvoke.UnhookWindowsHookEx(hookId);
            hookId = new HHOOK(IntPtr.Zero);
        }

        keyStates.Clear();
        IsEnabled = false;
    }

    private LRESULT hookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode < 0) return PInvoke.CallNextHookEx(hookId, nCode, wParam, lParam);

        var msg = (uint)wParam.Value;

        if (msg is not (wm_keydown or wm_syskeydown or wm_keyup or wm_syskeyup)) return PInvoke.CallNextHookEx(hookId, nCode, wParam, lParam);

        var info = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam.Value);
        var virtualKey = info.vkCode;

        var key = KeyInterop.KeyFromVirtualKey((int)virtualKey);
        var isKeyDown = msg is wm_keydown or wm_syskeydown;

        keyStates[key] = isKeyDown;

        return PInvoke.CallNextHookEx(hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        Disable();
        GC.SuppressFinalize(this);
    }

    private const uint wm_keydown = 0x0100;
    private const uint wm_keyup = 0x0101;
    private const uint wm_syskeydown = 0x0104;
    private const uint wm_syskeyup = 0x0105;
}