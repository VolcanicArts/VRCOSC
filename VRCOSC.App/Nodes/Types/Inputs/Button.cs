// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Inputs;

[Node("Call", "")]
public sealed class ButtonNode : Node
{
    public FlowContinuation Next = new();

    protected override async Task Process(PulseContext c)
    {
        await Next.Execute(c);
    }
}