// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.ProcessManager;

public class ProcessManagerModule : Module
{
    public override string Title => "Process Manager";
    public override string Description => "Allows for starting and stopping processes from avatar parameters";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.Integrations;

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(ProcessManagerParameter.Start, ParameterMode.Read, "VRCOSC/ProcessManager/Start/*", "Start", "Becoming true will start the process named in the '*' that you set on your avatar\nFor example, on your avatar you put: VRCOSC/ProcessManager/Start/vrchat");
        CreateParameter<bool>(ProcessManagerParameter.Stop, ParameterMode.Read, "VRCOSC/ProcessManager/Stop/*", "Stop", "Becoming true will stop the process named in the '*' that you set on your avatar\nFor example, on your avatar you put: VRCOSC/ProcessManager/Stop/vrchat");
    }

    protected override void OnBoolParameterReceived(Enum key, bool value, string[] wildcards)
    {
        var processName = wildcards[0];

        switch (key)
        {
            case ProcessManagerParameter.Start when value:
                startProcess(processName);
                break;

            case ProcessManagerParameter.Stop when value:
                stopProcess(processName);
                break;
        }
    }

    private void startProcess(string processName)
    {
        Log($"Attempting to start {processName}");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System),
                FileName = "cmd.exe",
                Arguments = $"/c start {processName}",
                UseShellExecute = true
            }
        };

        process.Start();
    }

    private async void stopProcess(string processName)
    {
        Log($"Attempting to stop {processName}");

        var processes = Process.GetProcessesByName(processName);

        if (!processes.Any())
        {
            Log($"Cannot find any process named {processName}");
            return;
        }

        foreach (var process in processes)
        {
            process.Kill();
            await process.WaitForExitAsync();
            process.Dispose();
        }
    }

    private enum ProcessManagerParameter
    {
        Start,
        Stop
    }
}
