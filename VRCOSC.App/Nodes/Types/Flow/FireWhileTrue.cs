// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire While True", "Flow")]
public sealed class FireWhileTrueNode : Node, IActiveUpdateNode
{
    public int UpdateOffset => 0;

    private readonly GlobalStore<DateTime> lastUpdate = new();

    public FlowCall Next = new();

    public ValueInput<int> DelayMilliseconds = new("Delay Milliseconds");
    public ValueInput<bool> Condition = new();

    protected override async Task Process(PulseContext c)
    {
        await Next.Execute(c);
    }

    public Task<bool> OnUpdate(PulseContext c)
    {
        var delay = DelayMilliseconds.Read(c);
        var dateTimeNow = DateTime.Now;
        var shouldContinue = (dateTimeNow - lastUpdate.Read(c)).TotalMilliseconds >= delay;

        if (shouldContinue && Condition.Read(c))
        {
            lastUpdate.Write(dateTimeNow, c);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}