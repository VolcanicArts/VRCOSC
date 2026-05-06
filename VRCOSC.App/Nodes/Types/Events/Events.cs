// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Events;

[Node("On Start", "Events")]
public sealed class OnStartNode : Node
{
    public FlowContinuation Next = new();

    protected override Task Process(PulseContext c) => Next.Execute(c);
}

[Node("On Stop", "Events")]
public sealed class OnStopNode : Node
{
    public FlowContinuation Next = new();

    protected override Task Process(PulseContext c) => Next.Execute(c);
}