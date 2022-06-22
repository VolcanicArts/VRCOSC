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

    protected void EnsureSingleTargetProcess()
    {
        var processes = Process.GetProcessesByName(TargetProcess);
        foreach (var process in processes[1..]) process.Kill();
    }

    protected bool IsTargetProcessOpen()
    {
        return isProcessOpen(TargetProcess);
    }

    protected void ExecuteKeyCombination(Enum lookup)
    {
        Task.Run(() => executeKeyCombination(lookup));
    }

    private void executeKeyCombination(Enum lookup)
    {
        switchToProcess(TargetProcess);
        performKeyCombination(lookup);
        switchToProcess(ReturnProcess);
    }

    private void switchToProcess(string processName)
    {
        if (string.IsNullOrEmpty(processName)) return;

        Process? process = retrieveProcess(processName);

        if (process == null)
        {
            Terminal.Log($"`{processName}` is not open");
            return;
        }

        focusProcess(process!);
    }

    private static bool isProcessOpen(string processName)
    {
        return retrieveProcess(processName) != null;
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
        Task.Delay(5);
    }

    private void performKeyCombination(Enum lookup)
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
