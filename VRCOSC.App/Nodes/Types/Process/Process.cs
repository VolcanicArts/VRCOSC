// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Process;

[Node("Is Process Open", "Process")]
public sealed class ProcessIsOpenNode : Node, IFlowInput
{
    public FlowContinuation Next = new();

    public ValueInput<string> Name = new();
    public ValueOutput<bool> IsOpen = new();

    protected override async Task Process(PulseContext c)
    {
        try
        {
            var name = Name.Read(c);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var processes = System.Diagnostics.Process.GetProcessesByName(name);
                IsOpen.Write(processes.Length != 0, c);
            }
        }
        catch
        {
        }

        await Next.Execute(c);
    }
}

[Node("Start Process", "Process")]
public sealed class ProcessStartNode : Node, IFlowInput
{
    public FlowContinuation OnStart = new();
    public FlowContinuation OnFail = new();

    public ValueInput<string?> Name = new();
    public ValueOutput<System.Diagnostics.Process?> ProcessOutput = new("Process");

    protected override async Task Process(PulseContext c)
    {
        try
        {
            var name = Name.Read(c);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System),
                        FileName = "cmd.exe",
                        Arguments = $"/c start {name}",
                        UseShellExecute = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                using var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_Process WHERE Name LIKE '%{name}%'"
                );

                var result = searcher.Get()
                                     .Cast<ManagementObject>()
                                     .OrderByDescending(p => p["CreationDate"]?.ToString() ?? "0")
                                     .FirstOrDefault();

                if (result != null)
                {
                    var pid = (int)(uint)result["ProcessId"];
                    var target = System.Diagnostics.Process.GetProcessById(pid);
                    ProcessOutput.Write(target, c);
                }

                await OnStart.Execute(c);
                return;
            }
        }
        catch
        {
        }

        await OnFail.Execute(c);
    }
}

[Node("Stop Process", "Process")]
public sealed class ProcessStopNode : Node, IFlowInput
{
    public FlowContinuation OnStop = new();
    public FlowContinuation OnFail = new();

    public ValueInput<string> Name = new();

    protected override async Task Process(PulseContext c)
    {
        try
        {
            var name = Name.Read(c);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var processes = System.Diagnostics.Process.GetProcessesByName(name);

                if (processes.Length == 0)
                {
                    await OnFail.Execute(c);
                    return;
                }

                foreach (var process in processes)
                {
                    process.Kill();
                    await process.WaitForExitAsync();
                    process.Dispose();
                }

                await OnStop.Execute(c);
                return;
            }
        }
        catch
        {
        }

        await OnFail.Execute(c);
    }
}