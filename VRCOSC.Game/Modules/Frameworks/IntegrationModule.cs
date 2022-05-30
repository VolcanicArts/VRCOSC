// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules.Frameworks;

public abstract class IntegrationModule : Module
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    private const int keyeventf_keyup = 0x0002;

    protected virtual IReadOnlyDictionary<Enum, WindowsVKey[]> KeyCombinations => new Dictionary<Enum, WindowsVKey[]>();
    protected virtual string TargetProcess => string.Empty;
    protected virtual string ReturnProcess => "vrchat";

    protected void ExecuteTask(Enum key)
    {
        switchToTarget();
        executeKeyCombination(key);
        switchToReturn();
    }

    private void switchToTarget()
    {
        if (!retrieveTargetProcess(out var targetProcess)) return;

        focusProcess(targetProcess!);
    }

    private void switchToReturn()
    {
        if (!retrieveReturnProcess(out var returnProcess)) return;

        focusProcess(returnProcess!);
    }

    private bool retrieveReturnProcess(out Process? returnProcess)
    {
        returnProcess = retrieveProcess(ReturnProcess);

        if (returnProcess != null) return true;

        Terminal.Log($"{ReturnProcess} cannot be found");
        return false;
    }

    private bool retrieveTargetProcess(out Process? targetProcess)
    {
        targetProcess = retrieveProcess(TargetProcess);

        if (targetProcess != null) return true;

        Terminal.Log($"{TargetProcess} cannot be found");
        return false;
    }

    private static Process? retrieveProcess(string processName)
    {
        return Process.GetProcessesByName(processName).FirstOrDefault();
    }

    private static void focusProcess(Process process)
    {
        if (process.MainWindowHandle == IntPtr.Zero)
        {
            ProcessHelper.ShowMainWindow(process, ShowWindowEnum.Restore);
        }

        ProcessHelper.SetMainWindowForeground(process);
    }

    private void executeKeyCombination(Enum combinationKey)
    {
        var keys = KeyCombinations[combinationKey];

        foreach (var key in keys)
        {
            holdKey((int)key);
        }

        foreach (var key in keys)
        {
            releaseKey((int)key);
        }
    }

    private static void holdKey(int key) => keybd_event((byte)key, (byte)key, 0, 0);
    private static void releaseKey(int key) => keybd_event((byte)key, (byte)key, keyeventf_keyup, 0);
}
