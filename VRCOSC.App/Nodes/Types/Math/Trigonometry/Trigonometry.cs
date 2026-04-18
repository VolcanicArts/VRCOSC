// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math.Trigonometry;

[Node("Sin", "Math/Trigonometry")]
[NodeCollapsed]
public sealed class SinNode<T>() : SimpleValueTransformNode<T>(T.Sin) where T : ITrigonometricFunctions<T>;

[Node("Cos", "Math/Trigonometry")]
[NodeCollapsed]
public sealed class CosNode<T>() : SimpleValueTransformNode<T>(T.Cos) where T : ITrigonometricFunctions<T>;

[Node("Tan", "Math/Trigonometry")]
[NodeCollapsed]
public sealed class TanNode<T>() : SimpleValueTransformNode<T>(T.Tan) where T : ITrigonometricFunctions<T>;

[Node("Asin", "Math/Trigonometry")]
[NodeCollapsed]
public sealed class AsinNode<T>() : SimpleValueTransformNode<T>(T.Asin) where T : ITrigonometricFunctions<T>;

[Node("Acos", "Math/Trigonometry")]
[NodeCollapsed]
public sealed class AcosNode<T>() : SimpleValueTransformNode<T>(T.Acos) where T : ITrigonometricFunctions<T>;

[Node("Atan", "Math/Trigonometry")]
[NodeCollapsed]
public sealed class AtanNode<T>() : SimpleValueTransformNode<T>(T.Atan) where T : ITrigonometricFunctions<T>;