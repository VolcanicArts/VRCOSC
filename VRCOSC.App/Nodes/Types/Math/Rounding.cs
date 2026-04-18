// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Round", "Math")]
public sealed class RoundNode<T> : ValueComputeNode<T> where T : IFloatingPoint<T>
{
    public ValueInput<T> Input = new();
    public ValueInput<int> DecimalPlaces = new();

    protected override T ComputeValue(PulseContext c)
    {
        var decimalPlaces = int.Clamp(DecimalPlaces.Read(c), 0, maxDigits<T>());
        return T.Round(Input.Read(c), decimalPlaces, MidpointRounding.AwayFromZero);
    }

    private static int maxDigits<Tf>() =>
        typeof(Tf) == typeof(float) ? 6 :
        typeof(Tf) == typeof(double) ? 15 :
        typeof(Tf) == typeof(decimal) ? 28 : 0;
}