// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Logging;
using VRCOSC.Game.Modules.Util;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public abstract class IntegrationModule : Module
{
    protected virtual string TargetProcess => string.Empty;
    protected virtual string ReturnProcess => "vrchat";
    protected virtual string TargetExe => $@"{TargetProcess}.exe";

    private readonly Dictionary<Enum, WindowsVKey[]> keyCombinations = new();

    protected void RegisterKeyCombination(Enum lookup, params WindowsVKey[] keys)
    {
        keyCombinations.Add(lookup, keys);
    }

    protected void StartTarget()
    {
        if (IsTargetProcessOpen()) return;

        try
        {
            Process.Start(TargetExe);
        }
        catch (Win32Exception e)
        {
            Terminal.Log($"`{TargetExe}` is not a valid path. You cannot start `{TargetProcess}` on start");
            Logger.Error(e, "IntegrationModule error");
        }
    }

    protected void StopTarget()
    {
        retrieveProcess(TargetProcess)?.Kill();
    }

    protected void ExecuteKeyCombination(Enum lookup)
    {
        Task.Run(() => executeKeyCombination(lookup));
    }

    private void executeKeyCombination(Enum lookup)
    {
        switchToTarget();
        processKeyCombination(lookup);
        switchToReturn();
    }

    protected bool IsTargetProcessOpen()
    {
        return isProcessValid(TargetProcess);
    }

    private bool isProcessValid(string processName)
    {
        return retrieveProcess(processName) != null;
    }

    private void switchToTarget()
    {
        if (string.IsNullOrEmpty(TargetProcess)) return;
        if (!retrieveTargetProcess(out var targetProcess)) return;

        focusProcess(targetProcess!);
    }

    private void switchToReturn()
    {
        if (string.IsNullOrEmpty(ReturnProcess)) return;
        if (!retrieveReturnProcess(out var returnProcess)) return;

        focusProcess(returnProcess!);
    }

    private bool retrieveReturnProcess(out Process? returnProcess)
    {
        returnProcess = retrieveProcess(ReturnProcess);
        return returnProcess != null;
    }

    private bool retrieveTargetProcess(out Process? targetProcess)
    {
        targetProcess = retrieveProcess(TargetProcess);
        return targetProcess != null;
    }

    private Process? retrieveProcess(string processName)
    {
        var process = Process.GetProcessesByName(processName).FirstOrDefault();

        if (process != null) return process;

        Terminal.Log($"`{processName}` cannot be found");
        return null;
    }

    private static void focusProcess(Process process)
    {
        if (process.MainWindowHandle == IntPtr.Zero)
        {
            ProcessHelper.ShowMainWindow(process, ShowWindowEnum.Restore);
        }

        Task.Delay(5);
        ProcessHelper.ShowMainWindow(process, ShowWindowEnum.ShowMaximized);
        Task.Delay(5);
        ProcessHelper.SetMainWindowForeground(process);
    }

    private void processKeyCombination(Enum lookup)
    {
        var keys = keyCombinations[lookup];

        foreach (var key in keys)
        {
            ProcessHelper.HoldKey((int)key);
        }

        Task.Delay(5);

        foreach (var key in keys)
        {
            ProcessHelper.ReleaseKey((int)key);
        }
    }
}
