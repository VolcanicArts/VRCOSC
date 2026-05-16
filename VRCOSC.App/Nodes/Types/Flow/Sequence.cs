// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Sequence", "Flow")]
public class SequenceNode : Node
{
    public FlowInput Input = new();
    public FlowOutputList Outputs = new();

    protected override async Task Process(IPulseContext c)
    {
        for (var i = 0; i < Outputs.Count; i++)
        {
            await Outputs.Execute(i, c);
        }
    }
}