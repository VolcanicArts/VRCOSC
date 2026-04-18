// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Abs", "Math")]
[NodeCollapsed]
public sealed class AbsoluteNode<T>() : SimpleValueTransformNode<T>(T.Abs) where T : INumberBase<T>;

[Node("Exp", "Math")]
[NodeCollapsed]
public sealed class ExponentialNode<T>() : SimpleValueTransformNode<T>(T.Exp) where T : IExponentialFunctions<T>;

[Node("Negate", "Math")]
[NodeCollapsed]
public sealed class NegateNode<T>() : SimpleValueTransformNode<T>(v => T.Abs(v) * -T.One) where T : INumberBase<T>;

[Node("Power", "Math", EFontAwesomeIcon.Solid_Superscript)]
public sealed class PowerNode<T>() : SimpleResultComputeNode<T>(T.Pow) where T : IFloatingPointIeee754<T>;

[Node("Square Root", "Math", EFontAwesomeIcon.Solid_SquareRootVariable)]
[NodeCollapsed]
public sealed class SquareRootNode<T>() : SimpleValueTransformNode<T>(T.Sqrt) where T : IRootFunctions<T>;

[Node("Cube Root", "Math")]
[NodeCollapsed]
public sealed class CubeRootNode<T>() : SimpleValueTransformNode<T>(T.Cbrt) where T : IRootFunctions<T>;

[Node("Reciprocal", "Math")]
[NodeCollapsed]
public sealed class ReciprocalNode<T>() : SimpleValueTransformNode<T>(v => T.One / v) where T : INumberBase<T>;

[Node("Ceil", "Math")]
[NodeCollapsed]
public sealed class CeilingNode<T>() : SimpleValueTransformNode<T>(T.Ceiling) where T : IFloatingPoint<T>;

[Node("Floor", "Math")]
[NodeCollapsed]
public sealed class FloorNode<T>() : SimpleValueTransformNode<T>(T.Floor) where T : IFloatingPoint<T>;