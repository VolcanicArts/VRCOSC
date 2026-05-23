// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Switch", "Flow")]
public sealed class SwitchNode<T> : Node
{
    public FlowInput FlowInput = new();
    public FlowOutput OnMiss = new("On Miss");
    public FlowOutputList OnMatchList = new("On Match");

    public ValueInputList<T> Values = new("Matches");

    [InputMode(InputModes.Connection)]
    public ValueInput<T> Value = new();

    // TODO: Add bool input for multiple matches?
    protected override async Task Process(IPulseContext c)
    {
        var value = Value.Read(c);
        var values = Values.Read(c);

        var index = -1;

        for (var i = 0; i < values.Count; i++)
        {
            var match = values[i];
            if (!EqualityComparer<T>.Default.Equals(value, match)) continue;

            index = i;
            break;
        }

        if (index == -1 || index >= OnMatchList.Count)
        {
            await OnMiss.Execute(c);
            return;
        }

        await OnMatchList.Execute(index, c);
    }
}