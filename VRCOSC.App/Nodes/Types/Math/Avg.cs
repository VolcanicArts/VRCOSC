// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Average", "Math")]
public class AverageNode<T> : Node where T : INumber<T>
{
    public ValueInputList<T> Inputs = new();
    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        var inputs = Inputs.Read(c);
        var value = inputs.Aggregate(T.Zero, (current, number) => current + number);
        Output.Write(value / T.CreateChecked(inputs.Count), c);
    }
}