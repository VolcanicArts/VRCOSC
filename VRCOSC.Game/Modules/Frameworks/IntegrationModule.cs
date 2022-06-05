// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules.Frameworks;

public abstract class IntegrationModule : Module
{
    protected virtual IReadOnlyDictionary<Enum, WindowsVKey[]> KeyCombinations => new Dictionary<Enum, WindowsVKey[]>();
    protected virtual string TargetProcess => string.Empty;
    protected virtual string ReturnProcess => "vrchat";

    protected void StartTarget()
    {
        var process = Process.Start($"{TargetProcess}.exe"); // TODO: Is the .exe needed?
        if (process.HasExited) Terminal.Log($"{TargetProcess} could not be found or is already running");
    }

    protected void StopTarget()
    {
        if (isValidProcess(TargetProcess)) retrieveProcess(TargetProcess + ".exe")?.Kill();
    }

    protected void RestartTarget()
    {
        StopTarget();
        StartTarget();
    }

    protected void ExecuteShortcut(Enum key)
    {
        switchToTarget();
        executeKeyCombination(key);
        switchToReturn();
    }

    private bool isValidProcess(string processName)
    {
        return retrieveProcess(processName) != null;
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

        ProcessHelper.ShowMainWindow(process, ShowWindowEnum.ShowMaximized);

        ProcessHelper.SetMainWindowForeground(process);
    }

    private void executeKeyCombination(Enum combinationKey)
    {
        var keys = KeyCombinations[combinationKey];

        foreach (var key in keys)
        {
            ProcessHelper.HoldKey((int)key);
        }

        foreach (var key in keys)
        {
            ProcessHelper.ReleaseKey((int)key);
        }
    }
}
