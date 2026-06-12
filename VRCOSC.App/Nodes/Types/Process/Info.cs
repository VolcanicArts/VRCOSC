// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Process;

[Node("Foreground Process", "Process")]
[NodeCollapsed]
public sealed class ForegroundProcessNode() : SimpleValueSourceNode<System.Diagnostics.Process?>(ProcessExtensions.GetForegroundProcess);

[Node("Unpack Process Info", "Process")]
public sealed class ProcessInfoNode() : ValueConsumeNode<System.Diagnostics.Process?>(nameof(Process)), IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<int> Pid = new("Id");
    public ValueOutput<string?> Name = new();
    public ValueOutput<DateTime> StartTime = new();

    protected override void ConsumeValue(System.Diagnostics.Process? process, IPulseContext c)
    {
        if (process is null) return;

        Pid.Write(process.Id, c);
        Name.Write(process.ProcessName, c);
        StartTime.Write(process.StartTime, c);
    }
}