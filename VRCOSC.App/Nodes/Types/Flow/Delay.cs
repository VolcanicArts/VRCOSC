// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Delay", "Flow")]
public sealed class DelayNode : AsyncActionNode
{
    public ValueInput<int> Milliseconds = new();

    protected override Task DoActionAsync(PulseContext c)
    {
        var milliseconds = Milliseconds.Read(c);
        return milliseconds > 0 ? Task.Delay(milliseconds, c.Token) : Task.CompletedTask;
    }
}