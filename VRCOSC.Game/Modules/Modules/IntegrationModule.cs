// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Logging;
using VRCOSC.Game.Processes;

namespace VRCOSC.Game.Modules.Modules;

public abstract partial class IntegrationModule : Module
{
    private const int delay = 10;

    protected virtual string TargetProcess => string.Empty;
    protected string ReturnProcess => "vrchat";
    protected string TargetExe => $@"{TargetProcess}.exe";

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
            Log($"`{TargetExe}` is not a valid path. You cannot start `{TargetProcess}` on start");
            Logger.Error(e, "IntegrationModule error");
        }
    }

    protected Process? GetTargetProgress()
    {
        return retrieveProcess(TargetProcess);
    }

    protected void StopTarget()
    {
        retrieveProcess(TargetProcess)?.Kill();
    }

    protected void EnsureSingleTargetProcess()
    {
        if (!IsTargetProcessOpen()) return;

        var processes = Process.GetProcessesByName(TargetProcess);
        foreach (var process in processes[1..]) process.Kill();
    }

    protected bool IsTargetProcessOpen()
    {
        return isProcessOpen(TargetProcess);
    }

    protected void ExecuteKeyCombination(Enum lookup)
    {
        _ = executeKeyCombination(lookup);
    }

    private async Task executeKeyCombination(Enum lookup)
    {
        await switchToProcess(TargetProcess);
        await Task.Delay(delay);
        await performKeyCombination(lookup);
        await Task.Delay(delay);
        await switchToProcess(ReturnProcess);
    }

    private async Task switchToProcess(string processName)
    {
        if (string.IsNullOrEmpty(processName)) return;

        Process? process = retrieveProcess(processName);

        if (process is null)
        {
            Log($"`{processName}` is not open");
            return;
        }

        await focusProcess(process);
    }

    private static bool isProcessOpen(string processName)
    {
        return retrieveProcess(processName) is not null;
    }

    private static Process? retrieveProcess(string processName)
    {
        return Process.GetProcessesByName(processName).FirstOrDefault();
    }

    private static async Task focusProcess(Process process)
    {
        if (process.MainWindowHandle == IntPtr.Zero)
        {
            process.ShowMainWindow(ShowWindowEnum.Restore);
            await Task.Delay(delay);
        }

        process.ShowMainWindow(ShowWindowEnum.ShowDefault);
        await Task.Delay(delay);
        process.SetMainWindowForeground();
    }

    private async Task performKeyCombination(Enum lookup)
    {
        await ProcessExtensions.PressKeys(keyCombinations[lookup], delay);
    }
}
