// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Process;

[Node("Is Process Open", "Process")]
public sealed class ProcessIsOpenNode : Node
{
    public FlowInput FlowInput = new();
    public FlowOutput Next = new();

    public ValueInput<string> Name = new();
    public ValueOutput<bool> IsOpen = new();

    protected override async Task Process(IPulseContext c)
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
public sealed class ProcessStartNode : TryActionAsyncNode
{
    public ValueInput<string?> Name = new();
    public ValueOutput<System.Diagnostics.Process?> ProcessOutput = new("Process");

    protected override async Task<bool> TryActionAsync(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return false;

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

        using var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE Name LIKE '%{name}%'");
        var result = searcher.Get().Cast<ManagementObject>().OrderByDescending(p => p["CreationDate"]?.ToString() ?? "0").FirstOrDefault();
        if (result == null) return false;

        var pid = (int)(uint)result["ProcessId"];
        var target = System.Diagnostics.Process.GetProcessById(pid);
        ProcessOutput.Write(target, c);

        return true;
    }
}

[Node("Stop Process", "Process")]
public sealed class ProcessStopNode : TryActionAsyncNode
{
    public ValueInput<string> Name = new();

    protected override async Task<bool> TryActionAsync(IPulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrWhiteSpace(name)) return false;

        var processes = System.Diagnostics.Process.GetProcessesByName(name);

        if (processes.Length == 0)
        {
            return false;
        }

        foreach (var process in processes)
        {
            process.Kill();
            await process.WaitForExitAsync();
            process.Dispose();
        }

        return true;
    }
}