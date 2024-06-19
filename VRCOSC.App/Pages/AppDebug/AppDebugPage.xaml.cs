// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Text;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.AppDebug;

public partial class AppDebugPage
{
    public Observable<string> Port9000BoundProcess { get; } = new(string.Empty);

    private readonly Repeater repeater;

    public AppDebugPage()
    {
        InitializeComponent();
        DataContext = this;

        repeater = new Repeater(() => { Port9000BoundProcess.Value = $"Port 9000 is bound to {executePowerShellCommand("Get-Process -Id (Get-NetUDPEndpoint -LocalPort 9000).OwningProcess | Select-Object -ExpandProperty ProcessName") ?? "Nothing"}"; });
        repeater.Start(TimeSpan.FromSeconds(5), true);
    }

    private static string? executePowerShellCommand(string command)
    {
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
}
