// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Clamp", "Math/Clamp")]
public sealed class ClampNode<T> : ValueComputeNode<T> where T : INumber<T>
{
    public ValueInput<T> Value = new();
    public ValueInput<T> Min = new();
    public ValueInput<T> Max = new();

    protected override T ComputeValue(IPulseContext c) => T.Clamp(Value.Read(c), Min.Read(c), Max.Read(c));
}

[Node("Clamp 0,1", "Math/Clamp")]
[NodeCollapsed]
public sealed class Clamp01Node<T>() : SimpleValueTransformNode<T>(v => T.Clamp(v, T.Zero, T.One)) where T : IFloatingPoint<T>;