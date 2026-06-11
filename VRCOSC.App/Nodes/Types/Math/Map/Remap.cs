// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math.Map;

[Node("Remap", "Math/Map")]
public class RemapNode<TFrom, TTo> : ValueComputeNode<TTo> where TFrom : INumber<TFrom> where TTo : INumber<TTo>
{
    [InputMode(InputModes.Connection)]
    public ValueInput<TFrom> Value = new();

    public ValueInput<TFrom> FromMin = new();
    public ValueInput<TFrom> FromMax = new();
    public ValueInput<TTo> ToMin = new();
    public ValueInput<TTo> ToMax = new();
    public ValueInput<bool> Clamp = new();

    protected override TTo ComputeValue(IPulseContext c)
    {
        var value = Value.Read(c);
        var fromMin = FromMin.Read(c);
        var fromMax = FromMax.Read(c);

        if (Clamp.Read(c))
            value = TFrom.Clamp(value, fromMin, fromMax);

        return Utils.Interpolation.Map(value, fromMin, fromMax, ToMin.Read(c), ToMax.Read(c));
    }
}

public sealed class RemapNode<T> : RemapNode<T, T> where T : INumber<T>;

[Node("Remap 0,1 To -1,1", "Math/Map")]
[NodeCollapsed]
public sealed class Remap0111Node<T>() : SimpleValueTransformNode<T>(v => Utils.Interpolation.Map(v, T.Zero, T.One, T.NegativeOne, T.One)) where T : IFloatingPoint<T>
{
    public override string DisplayName => "0,1 -> -1,1";
}

[Node("Remap -1,1 To 0,1", "Math/Map")]
[NodeCollapsed]
public sealed class Remap1101Node<T>() : SimpleValueTransformNode<T>(v => Utils.Interpolation.Map(v, T.NegativeOne, T.One, T.Zero, T.One)) where T : IFloatingPoint<T>
{
    public override string DisplayName => "-1,1 -> 0,1";
}