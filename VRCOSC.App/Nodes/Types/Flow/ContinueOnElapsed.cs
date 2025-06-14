// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Continue On Elapsed", "Flow")]
public sealed class ContinueOnElapsedNode : Node, IFlowInput
{
    private readonly GlobalStore<DateTime> lastUpdate = new();

    public FlowContinuation OnElapsed = new("On Elapsed");

    public ValueInput<int> ElapsedMilliseconds = new("Elapsed Milliseconds");

    protected override void Process(PulseContext c)
    {
        var dateTimeNow = DateTime.Now;

        if ((dateTimeNow - lastUpdate.Read(c)).TotalMilliseconds >= ElapsedMilliseconds.Read(c))
        {
            lastUpdate.Write(dateTimeNow, c);
            OnElapsed.Execute(c);
        }
    }
}