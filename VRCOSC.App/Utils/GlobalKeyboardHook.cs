// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace VRCOSC.App.Utils;

internal class GlobalKeyboardHook : IDisposable
{
    private readonly HOOKPROC hookProc;
    private HHOOK hookId;

    private Thread? hookThread;
    private uint hookNativeThreadId;
    private readonly ManualResetEventSlim hookReady = new();
    private volatile bool stopRequested;

    private readonly Dictionary<Key, bool> keyStates = new();
    private readonly Lock stateLock = new();

    public bool IsEnabled { get; private set; }

    public GlobalKeyboardHook()
    {
        hookProc = hookCallback;
        hookId = new HHOOK(IntPtr.Zero);
    }

    public bool GetKeyState(Key key)
    {
        lock (stateLock)
            return keyStates.GetValueOrDefault(key, false);
    }

    public unsafe void Enable()
    {
        if (IsEnabled) return;

        stopRequested = false;
        hookReady.Reset();

        hookThread = new Thread(hookThreadMain)
        {
            IsBackground = true,
            Name = $"{AppManager.APP_NAME} {nameof(GlobalKeyboardHook)}",
            Priority = ThreadPriority.Highest
        };

        try
        {
            hookThread.SetApartmentState(ApartmentState.STA);
        }
        catch
        {
        }

        hookThread.Start();

        if (!hookReady.Wait(TimeSpan.FromSeconds(2)))
            throw new TimeoutException("Timed out waiting for global keyboard hook to initialize");

        if (hookId.Value == (void*)IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to set global keyboard hook");

        IsEnabled = true;
    }

    public void Disable()
    {
        if (!IsEnabled) return;

        stopRequested = true;

        if (hookNativeThreadId != 0)
        {
            PInvoke.PostThreadMessage(hookNativeThreadId, wm_quit, new WPARAM(), new LPARAM());
        }

        try
        {
            hookThread?.Join(TimeSpan.FromSeconds(2));
        }
        catch
        {
        }

        hookThread = null;
        hookNativeThreadId = 0;

        lock (stateLock) keyStates.Clear();

        IsEnabled = false;
    }

    private unsafe void hookThreadMain()
    {
        hookNativeThreadId = PInvoke.GetCurrentThreadId();

        hookId = PInvoke.SetWindowsHookEx(
            WINDOWS_HOOK_ID.WH_KEYBOARD_LL,
            hookProc,
            HINSTANCE.Null,
            0
        );

        hookReady.Set();

        if (hookId.Value == (void*)IntPtr.Zero) return;

        while (PInvoke.GetMessage(out var msg, HWND.Null, 0, 0))
        {
            if (stopRequested && msg.message == wm_quit)
                break;

            PInvoke.TranslateMessage(msg);
            PInvoke.DispatchMessage(msg);
        }

        if (hookId.Value == (void*)IntPtr.Zero) return;

        PInvoke.UnhookWindowsHookEx(hookId);
        hookId = new HHOOK(IntPtr.Zero);
    }

    private LRESULT hookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        var msg = (uint)wParam.Value;

        if (msg is not (wm_keydown or wm_syskeydown or wm_keyup or wm_syskeyup))
            return PInvoke.CallNextHookEx(hookId, nCode, wParam, lParam);

        var info = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam.Value);
        var key = KeyInterop.KeyFromVirtualKey((int)info.vkCode);
        var isKeyDown = msg is wm_keydown or wm_syskeydown;

        lock (stateLock)
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
    private const uint wm_quit = 0x0012;
}