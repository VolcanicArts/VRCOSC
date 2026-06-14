// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

public abstract class FireWhileBase(Func<bool, bool> checkCondition) : Node, IActiveUpdateNode
{
    public int UpdateOffset => 0;

    public GlobalStore<double> LastUpdateStore = new();

    public FlowOutput Next = new();

    public ValueInput<int> Delay = new("Delay (ms)");
    public ValueInput<bool> Condition = new();

    protected override Task Process(IPulseContext c) => Next.Execute(c);

    public Task<bool> OnUpdate(IPulseContext c)
    {
        var delay = Delay.Read(c);
        var condition = Condition.Read(c);

        if (checkCondition(condition))
        {
            var accumulated = LastUpdateStore.Read(c);
            accumulated += c.DeltaTime;

            if (accumulated >= delay)
            {
                LastUpdateStore.Write(accumulated - delay, c);
                return Task.FromResult(true);
            }

            LastUpdateStore.Write(accumulated, c);
            return Task.FromResult(false);
        }

        LastUpdateStore.Write(0, c);
        return Task.FromResult(false);
    }
}

[Node("Fire While True", "Flow")]
public sealed class FireWhileTrueNode() : FireWhileBase(v => v);

[Node("Fire While False", "Flow")]
public sealed class FireWhileFalseNode() : FireWhileBase(v => !v);