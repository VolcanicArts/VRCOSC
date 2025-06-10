// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Delay", "Flow")]
public sealed class DelayNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<int> Milliseconds = new("Milliseconds");

    protected override void Process(PulseContext c)
    {
        Task.Delay(Milliseconds.Read(c), c.Token).Wait(c.Token);
        Next.Execute(c);
    }
}