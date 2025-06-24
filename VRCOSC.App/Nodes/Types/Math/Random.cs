// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Random", "Math")]
public sealed class RandomNode<T> : Node where T : INumber<T>
{
    private readonly Random random = new();

    public ValueInput<T> Min = new();
    public ValueInput<T> Max = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        var minDouble = double.CreateChecked(Min.Read(c));
        var maxDouble = double.CreateChecked(Max.Read(c));

        var valueDouble = minDouble + (maxDouble - minDouble) * random.NextDouble();

        var value = T.CreateChecked(valueDouble);
        Result.Write(value, c);
        return Task.CompletedTask;
    }
}