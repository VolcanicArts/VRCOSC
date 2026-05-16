// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Continue On Elapsed", "Flow")]
public sealed class ContinueOnElapsedNode : Node
{
    public GlobalStore<DateTime> LastUpdateStore = new();

    public FlowInput FlowInput = new();
    public FlowOutput OnElapsed = new();

    public ValueInput<int> ElapsedMilliseconds = new();

    protected override async Task Process(IPulseContext c)
    {
        var elapsedMilliseconds = ElapsedMilliseconds.Read(c);

        if (elapsedMilliseconds <= 0) return;

        var dateTimeNow = DateTime.Now;

        if ((dateTimeNow - LastUpdateStore.Read(c)).TotalMilliseconds >= elapsedMilliseconds)
        {
            LastUpdateStore.Write(dateTimeNow, c);
            await OnElapsed.Execute(c);
        }
    }
}