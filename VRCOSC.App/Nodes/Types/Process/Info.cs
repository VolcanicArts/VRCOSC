// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Process;

[Node("Foreground Process", "Process")]
[NodeCollapsed]
public sealed class ForegroundProcessNode() : ValueSourceNode<System.Diagnostics.Process?>(ProcessExtensions.GetForegroundProcess);

[Node("Process Info", "Process/Info")]
public sealed class ProcessInfoNode : UpdateNode<int, string?, DateTime>
{
    public ValueInput<System.Diagnostics.Process?> ProcessInput = new("Process");

    public ValueOutput<int> Pid = new("Id");
    public ValueOutput<string?> Name = new();
    public ValueOutput<DateTime> StartTime = new();

    protected override Task Process(PulseContext c)
    {
        var process = ProcessInput.Read(c);
        if (process is null) return Task.CompletedTask;

        Pid.Write(process.Id, c);
        Name.Write(process.ProcessName, c);
        StartTime.Write(process.StartTime, c);
        return Task.CompletedTask;
    }

    protected override Task<(int, string?, DateTime)> GetValues(PulseContext c)
    {
        var process = ProcessInput.Read(c);
        if (process is null) return Task.FromResult((0, (string?)null, DateTime.UnixEpoch));

        return Task.FromResult((process.Id, (string?)process.ProcessName, process.StartTime));
    }
}