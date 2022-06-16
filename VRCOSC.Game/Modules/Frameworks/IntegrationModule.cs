// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules.Frameworks;

public abstract class IntegrationModule : Module
{
    public virtual string TargetProcess => string.Empty;
    public virtual string ReturnProcess => "vrchat";
    public virtual string TargetExe => $@"{TargetProcess}.exe";
    public readonly Dictionary<Enum, WindowsVKey[]> KeyCombinations = new();

    protected void StartTarget()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = TargetExe,
            UseShellExecute = false
        };

        try
        {
            var process = Process.Start(startInfo);
            if (process == null || process!.HasExited) Terminal.Log($"{TargetExe} could not be found or is already running");
        }
        catch (Win32Exception)
        {
            Terminal.Log($"{TargetExe} is not a valid path. You cannot start {TargetProcess} on start");
        }
    }

    protected void RegisterKeyCombination(Enum lookup, params WindowsVKey[] keys)
    {
        KeyCombinations.Add(lookup, keys);
    }

    protected void StopTarget()
    {
        if (isValidProcess(TargetProcess)) retrieveProcess(TargetExe)?.Kill();
    }

    protected void RestartTarget()
    {
        StopTarget();
        StartTarget();
    }

    protected void ExecuteShortcut(Enum key)
    {
        Task.Run(() =>
        {
            if (!string.IsNullOrEmpty(TargetProcess)) switchToTarget();
            executeKeyCombination(key);
            if (!string.IsNullOrEmpty(ReturnProcess)) switchToReturn();
        });
    }

    protected bool IsProcessOpen()
    {
        // Returns the first of any processes that match the target process name
        // E.g. if you had multiple "calc" processes open, this would not always fetch the correct one
        return Process.GetProcesses().Any(p => p.ProcessName.Contains(TargetProcess));
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

        Task.Delay(5);

        ProcessHelper.ShowMainWindow(process, ShowWindowEnum.ShowMaximized);

        Task.Delay(5);

        ProcessHelper.SetMainWindowForeground(process);
    }

    private void executeKeyCombination(Enum combinationKey)
    {
        var keys = KeyCombinations[combinationKey];

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
