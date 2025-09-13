// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire While False", "Flow")]
public sealed class FireWhileFalseNode : Node
{
    public FlowCall Next = new("Next");

    [NodeReactive]
    public ValueInput<int> DelayMilliseconds = new("Delay Milliseconds");

    [NodeReactive]
    public ValueInput<bool> Condition = new();

    protected override async Task Process(PulseContext c)
    {
        var delay = DelayMilliseconds.Read(c);

        if (Condition.Read(c) || delay <= 0) return;

        while (!c.IsCancelled)
        {
            await Next.Execute(c);
            if (c.IsCancelled) break;

            await Task.Delay(delay, c.Token);
            if (c.IsCancelled) break;
        }
    }

    protected override bool ShouldProcess(PulseContext c) => true;
}