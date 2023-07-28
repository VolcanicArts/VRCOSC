// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;

namespace VRCOSC.Modules.ProcessManager;

[ModuleTitle("Process Manager")]
[ModuleDescription("Allows for starting and stopping processes from avatar parameters")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.Integrations)]
public class ProcessManagerModule : AvatarModule
{
    protected override void CreateAttributes()
    {
        CreateParameter<bool>(ProcessManagerParameter.Start, ParameterMode.Read, "VRCOSC/ProcessManager/Start/*", "Start", "Becoming true will start the process named in the '*' that you set on your avatar\nFor example, on your avatar you put: VRCOSC/ProcessManager/Start/vrchat");
        CreateParameter<bool>(ProcessManagerParameter.Stop, ParameterMode.Read, "VRCOSC/ProcessManager/Stop/*", "Stop", "Becoming true will stop the process named in the '*' that you set on your avatar\nFor example, on your avatar you put: VRCOSC/ProcessManager/Stop/vrchat");
    }

    protected override void OnRegisteredParameterReceived(AvatarParameter parameter)
    {
        var processName = parameter.WildcardAs<string>(0);

        switch (parameter.Lookup)
        {
            case ProcessManagerParameter.Start when parameter.ValueAs<bool>():
                startProcess(processName);
                break;

            case ProcessManagerParameter.Stop when parameter.ValueAs<bool>():
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
