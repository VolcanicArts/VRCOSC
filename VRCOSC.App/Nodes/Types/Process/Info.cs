// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Process;

[Node("Foreground Process", "Process/Info")]
[NodeCollapsed]
public sealed class ForegroundProcessNode() : ValueSourceNode<System.Diagnostics.Process?>(ProcessExtensions.GetForegroundProcess);

[Node("Process Info", "Process/Info")]
public sealed class ProcessInfoNode : Node
{
    public ValueInput<System.Diagnostics.Process?> ProcessInput = new();

    public ValueOutput<string> Name = new();

    protected override Task Process(PulseContext c)
    {
        var process = ProcessInput.Read(c);
        if (process is null) return Task.CompletedTask;

        Name.Write(process.ProcessName, c);
        return Task.CompletedTask;
    }
}