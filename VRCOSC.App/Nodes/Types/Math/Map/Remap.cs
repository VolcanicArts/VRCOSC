// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math.Map;

[Node("Remap", "Math/Map")]
public sealed class RemapNode<TFrom, TTo> : ValueComputeNode<TTo> where TFrom : INumber<TFrom> where TTo : INumber<TTo>
{
    public ValueInput<TFrom> Value = new();
    public ValueInput<TFrom> FromMin = new();
    public ValueInput<TFrom> FromMax = new();
    public ValueInput<TTo> ToMin = new();
    public ValueInput<TTo> ToMax = new();

    protected override TTo ComputeValue(PulseContext c) => Utils.Interpolation.Map(Value.Read(c), FromMin.Read(c), FromMax.Read(c), ToMin.Read(c), ToMax.Read(c));
}

[Node("Remap 0,1 To -1,1", "Math/Map")]
[NodeCollapsed]
public sealed class Remap0111Node<T>() : SimpleValueTransformNode<T>(v => Utils.Interpolation.Map(v, T.Zero, T.One, T.NegativeOne, T.One)) where T : IFloatingPoint<T>;

[Node("Remap -1,1 To 0,1", "Math/Map")]
[NodeCollapsed]
public sealed class Remap1101Node<T>() : SimpleValueTransformNode<T>(v => Utils.Interpolation.Map(v, T.NegativeOne, T.One, T.Zero, T.One)) where T : IFloatingPoint<T>;