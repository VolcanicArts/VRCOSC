// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules.Frameworks;

public abstract class IntegrationModule : Module
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    private const int keyeventf_keyup = 0x0002;

    protected virtual IReadOnlyDictionary<Enum, int[]> KeyCombinations => new Dictionary<Enum, int[]>();
    protected virtual string TargetProcess => string.Empty;
    protected virtual string ReturnProcess => "vrchat";

    protected async Task ExecuteTask(Enum key)
    {
        await switchToTarget();
        await executeKeyCombination(key);
        await switchToReturn();
    }

    private async Task switchToTarget()
    {
        if (!retrieveTargetProcess(out var targetProcess)) return;

        await focusProcess(targetProcess!);
    }

    private async Task switchToReturn()
    {
        if (!retrieveReturnProcess(out var returnProcess)) return;

        await focusProcess(returnProcess!);
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

    private Process? retrieveProcess(string processName)
    {
        return Process.GetProcessesByName(processName).FirstOrDefault();
    }

    private static async Task focusProcess(Process process)
    {
        if (process.MainWindowHandle == IntPtr.Zero)
        {
            await Task.Delay(10);
            ProcessHelper.ShowMainWindow(process, ShowWindowEnum.Restore);
        }

        await Task.Delay(10);
        ProcessHelper.ShowMainWindow(process, ShowWindowEnum.ShowMaximized);
        await Task.Delay(10);
        ProcessHelper.SetMainWindowForeground(process);
        await Task.Delay(10);
    }

    private async Task executeKeyCombination(Enum combinationKey)
    {
        var keys = KeyCombinations[combinationKey];

        foreach (var key in keys)
        {
            holdKey(key);
        }

        await Task.Delay(10);

        foreach (var key in keys)
        {
            releaseKey(key);
        }
    }

    private static void holdKey(int key) => keybd_event((byte)key, (byte)key, 0, 0);
    private static void releaseKey(int key) => keybd_event((byte)key, (byte)key, keyeventf_keyup, 0);
}
