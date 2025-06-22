// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire While True", "Flow")]
public sealed class FireWhileTrueNode : Node
{
    public GlobalStore<bool> PrevCondition = new();
    public GlobalStore<int> PrevDelay = new();

    public FlowCall IsTrue = new("Is True");

    [NodeReactive]
    public ValueInput<int> DelayMilliseconds = new("Delay Milliseconds");

    [NodeReactive]
    public ValueInput<bool> Condition = new();

    protected override void Process(PulseContext c)
    {
        PrevCondition.Write(Condition.Read(c), c);
        PrevDelay.Write(DelayMilliseconds.Read(c), c);

        var delay = DelayMilliseconds.Read(c);

        if (!Condition.Read(c) || delay <= 0) return;

        while (!c.IsCancelled)
        {
            IsTrue.Execute(c);
            if (c.IsCancelled) break;

            Task.Delay(delay, c.Token).Wait(c.Token);
            if (c.IsCancelled) break;
        }
    }

    protected override bool ShouldProcess(PulseContext c) => PrevCondition.Read(c) != Condition.Read(c) || PrevDelay.Read(c) != DelayMilliseconds.Read(c);
}