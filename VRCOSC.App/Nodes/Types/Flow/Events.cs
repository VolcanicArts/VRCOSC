// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("On Start", "Flow/Events")]
public class OnStartNode : Node, INodeEventHandler
{
    public FlowCall OnStart = new("On Start");

    protected override void Process(PulseContext c)
    {
        OnStart.Execute(c);
    }

    public bool HandleNodeStart(PulseContext c) => true;
}

[Node("On Stop", "Flow/Events")]
public class OnStopNode : Node, INodeEventHandler
{
    public FlowCall OnStop = new("On Stop");

    protected override void Process(PulseContext c)
    {
        OnStop.Execute(c);
    }

    public bool HandleNodeStop(PulseContext c) => true;
}