// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using VRCOSC.App.Modules;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.AppDebug;

public partial class AppDebugView
{
    public Observable<string> Port9000BoundProcess { get; } = new(string.Empty);

    private readonly Repeater repeater;

    public AppDebugView()
    {
        InitializeComponent();
        DataContext = this;

        repeater = new Repeater($"{nameof(AppDebugView)}-{nameof(executePowerShellCommand)}", () => Port9000BoundProcess.Value = $"{(executePowerShellCommand("Get-Process -Id (Get-NetUDPEndpoint -LocalPort 9000).OwningProcess | Select-Object -ExpandProperty ProcessName") ?? "Nothing").ReplaceLineEndings(string.Empty)}");
        repeater.Start(TimeSpan.FromSeconds(5), true);
    }

    private static string? executePowerShellCommand(string command)
    {
        if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.EnableAppDebug)) return string.Empty;

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();

        process.StartInfo = psi;

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) output.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) error.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return error.Length > 0 ? null : output.ToString();
    }

    private async void ReloadModules_OnClick(object sender, RoutedEventArgs e)
    {
        await ModuleManager.GetInstance().ReloadAllModules();
    }
}