// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Delay", "Flow")]
public sealed class DelayNode : AsyncActionNode
{
    public ValueInput<int> Milliseconds = new();

    protected override Task DoActionAsync(IPulseContext c)
    {
        var milliseconds = Milliseconds.Read(c);
        return milliseconds > 0 ? c.Run(Task.Delay(milliseconds)) : Task.CompletedTask;
    }
}