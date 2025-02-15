// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VRCOSC.App.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class AppDebugView
{
    public Observable<string> Port9000BoundProcess { get; } = new(string.Empty);

    private DTWrapper? port9000DispatcherTimer;

    public AppDebugView()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        port9000DispatcherTimer = new DTWrapper("Debug-Port9000", TimeSpan.FromSeconds(5), true, updatePort9000Process);
        port9000DispatcherTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        port9000DispatcherTimer?.Stop();
        port9000DispatcherTimer = null;
    }

    private void updatePort9000Process() => Task.Run(async () =>
    {
        var boundProcess = await executePowerShellCommand("Get-Process -Id (Get-NetUDPEndpoint -LocalPort 9000).OwningProcess | Select-Object -ExpandProperty ProcessName");
        Dispatcher.Invoke(() => Port9000BoundProcess.Value = $"{(boundProcess ?? "Nothing").ReplaceLineEndings(string.Empty)}");
    });

    private static async Task<string?> executePowerShellCommand(string command)
    {
        try
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
            await process.WaitForExitAsync();

            return error.Length > 0 ? null : output.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async void ReloadModules_OnClick(object sender, RoutedEventArgs e)
    {
        await ModuleManager.GetInstance().ReloadAllModules();
    }
}