// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire While False", "Flow")]
public sealed class FireWhileFalseNode : Node, IActiveUpdateNode
{
    public int UpdateOffset => 0;

    public GlobalStore<DateTime> LastUpdateStore = new();

    public FlowContinuation Next = new();

    public ValueInput<int> DelayMilliseconds = new("Delay Milliseconds");
    public ValueInput<bool> Condition = new();

    protected override async Task Process(PulseContext c)
    {
        LastUpdateStore.Write(DateTime.Now, c);
        await Next.Execute(c);
    }

    public Task<bool> OnUpdate(PulseContext c)
    {
        var delay = DelayMilliseconds.Read(c);
        var shouldContinue = (DateTime.Now - LastUpdateStore.Read(c)).TotalMilliseconds >= delay;
        return Task.FromResult(shouldContinue && !Condition.Read(c));
    }
}