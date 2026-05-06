// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Average", "Math")]
public sealed class AverageNode<T> : ValueComputeNode<T> where T : INumber<T>
{
    public ValueInputList<T> Inputs = new();

    protected override T ComputeValue(PulseContext c)
    {
        var inputs = Inputs.Read(c);
        var value = inputs.Aggregate(T.Zero, (current, number) => current + number);
        return value / T.CreateSaturating(inputs.Count);
    }
}