// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

public abstract class FireWhileBase(Func<bool, bool> checkCondition) : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public GlobalStore<DateTime> LastUpdateStore = new();

    public FlowOutput Next = new();

    public ValueInput<int> Delay = new("Delay (ms)", 10);
    public ValueInput<bool> Condition = new();

    protected override Task Process(IPulseContext c)
    {
        LastUpdateStore.Write(DateTime.Now, c);
        return Next.Execute(c);
    }

    protected override bool ShouldProcess(IPulseContext c)
    {
        var delay = Delay.Read(c);
        var shouldContinue = (DateTime.Now - LastUpdateStore.Read(c)).TotalMilliseconds >= delay;
        return shouldContinue && checkCondition(Condition.Read(c));
    }
}

[Node("Fire While True", "Flow")]
public sealed class FireWhileTrueNode() : FireWhileBase(v => v);

[Node("Fire While False", "Flow")]
public sealed class FireWhileFalseNode() : FireWhileBase(v => !v);