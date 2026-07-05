// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Ease", "Math")]
public sealed class EaseNode<T> : ValueComputeNode<T> where T : IFloatingPointIeee754<T>
{
    [InputMode(InputModes.Connection)]
    public ValueInput<T> Value = new();

    public ValueInput<EasingMode> Mode = new();

    protected override T ComputeValue(IPulseContext c) => Utils.Easing.Apply(Value.Read(c), Mode.Read(c));
}

[NodeCollapsed]
public abstract class EaseBase<T>(Func<T, T> func) : SimpleValueTransformNode<T>(func) where T : IFloatingPointIeee754<T>;

[Node("Quadratic In", "Math/Easing")]
public sealed class QuadraticInNode<T>() : EaseBase<T>(Utils.Easing.Quadratic.In) where T : IFloatingPointIeee754<T>;

[Node("Quadratic Out", "Math/Easing")]
public sealed class QuadraticOutNode<T>() : EaseBase<T>(Utils.Easing.Quadratic.Out) where T : IFloatingPointIeee754<T>;

[Node("Quadratic InOut", "Math/Easing")]
public sealed class QuadraticInOutNode<T>() : EaseBase<T>(Utils.Easing.Quadratic.InOut) where T : IFloatingPointIeee754<T>;

[Node("Cubic In", "Math/Easing")]
public sealed class CubicInNode<T>() : EaseBase<T>(Utils.Easing.Cubic.In) where T : IFloatingPointIeee754<T>;

[Node("Cubic Out", "Math/Easing")]
public sealed class CubicOutNode<T>() : EaseBase<T>(Utils.Easing.Cubic.Out) where T : IFloatingPointIeee754<T>;

[Node("Cubic InOut", "Math/Easing")]
public sealed class CubicInOutNode<T>() : EaseBase<T>(Utils.Easing.Cubic.InOut) where T : IFloatingPointIeee754<T>;

[Node("Quartic In", "Math/Easing")]
public sealed class QuarticInNode<T>() : EaseBase<T>(Utils.Easing.Quartic.In) where T : IFloatingPointIeee754<T>;

[Node("Quartic Out", "Math/Easing")]
public sealed class QuarticOutNode<T>() : EaseBase<T>(Utils.Easing.Quartic.Out) where T : IFloatingPointIeee754<T>;

[Node("Quartic InOut", "Math/Easing")]
public sealed class QuarticInOutNode<T>() : EaseBase<T>(Utils.Easing.Quartic.InOut) where T : IFloatingPointIeee754<T>;

[Node("Quintic In", "Math/Easing")]
public sealed class QuinticInNode<T>() : EaseBase<T>(Utils.Easing.Quintic.In) where T : IFloatingPointIeee754<T>;

[Node("Quintic Out", "Math/Easing")]
public sealed class QuinticOutNode<T>() : EaseBase<T>(Utils.Easing.Quintic.Out) where T : IFloatingPointIeee754<T>;

[Node("Quintic InOut", "Math/Easing")]
public sealed class QuinticInOutNode<T>() : EaseBase<T>(Utils.Easing.Quintic.InOut) where T : IFloatingPointIeee754<T>;

[Node("Sine In", "Math/Easing")]
public sealed class SineInNode<T>() : EaseBase<T>(Utils.Easing.Sinusoidal.In) where T : IFloatingPointIeee754<T>;

[Node("Sine Out", "Math/Easing")]
public sealed class SineOutNode<T>() : EaseBase<T>(Utils.Easing.Sinusoidal.Out) where T : IFloatingPointIeee754<T>;

[Node("Sine InOut", "Math/Easing")]
public sealed class SineInOutNode<T>() : EaseBase<T>(Utils.Easing.Sinusoidal.InOut) where T : IFloatingPointIeee754<T>;

[Node("Exponential In", "Math/Easing")]
public sealed class ExponentialInNode<T>() : EaseBase<T>(Utils.Easing.Exponential.In) where T : IFloatingPointIeee754<T>;

[Node("Exponential Out", "Math/Easing")]
public sealed class ExponentialOutNode<T>() : EaseBase<T>(Utils.Easing.Exponential.Out) where T : IFloatingPointIeee754<T>;

[Node("Exponential InOut", "Math/Easing")]
public sealed class ExponentialInOutNode<T>() : EaseBase<T>(Utils.Easing.Exponential.InOut) where T : IFloatingPointIeee754<T>;

[Node("Circular In", "Math/Easing")]
public sealed class CircularInNode<T>() : EaseBase<T>(Utils.Easing.Circular.In) where T : IFloatingPointIeee754<T>;

[Node("Circular Out", "Math/Easing")]
public sealed class CircularOutNode<T>() : EaseBase<T>(Utils.Easing.Circular.Out) where T : IFloatingPointIeee754<T>;

[Node("Circular InOut", "Math/Easing")]
public sealed class CircularInOutNode<T>() : EaseBase<T>(Utils.Easing.Circular.InOut) where T : IFloatingPointIeee754<T>;

[Node("Elastic In", "Math/Easing")]
public sealed class ElasticInNode<T>() : EaseBase<T>(Utils.Easing.Elastic.In) where T : IFloatingPointIeee754<T>;

[Node("Elastic Out", "Math/Easing")]
public sealed class ElasticOutNode<T>() : EaseBase<T>(Utils.Easing.Elastic.Out) where T : IFloatingPointIeee754<T>;

[Node("Elastic InOut", "Math/Easing")]
public sealed class ElasticInOutNode<T>() : EaseBase<T>(Utils.Easing.Elastic.InOut) where T : IFloatingPointIeee754<T>;

[Node("Back In", "Math/Easing")]
public sealed class BackInNode<T>() : EaseBase<T>(Utils.Easing.Back.In) where T : IFloatingPointIeee754<T>;

[Node("Back Out", "Math/Easing")]
public sealed class BackOutNode<T>() : EaseBase<T>(Utils.Easing.Back.Out) where T : IFloatingPointIeee754<T>;

[Node("Back InOut", "Math/Easing")]
public sealed class BackInOutNode<T>() : EaseBase<T>(Utils.Easing.Back.InOut) where T : IFloatingPointIeee754<T>;

[Node("Bounce In", "Math/Easing")]
public sealed class BounceInNode<T>() : EaseBase<T>(Utils.Easing.Bounce.In) where T : IFloatingPointIeee754<T>;

[Node("Bounce Out", "Math/Easing")]
public sealed class BounceOutNode<T>() : EaseBase<T>(Utils.Easing.Bounce.Out) where T : IFloatingPointIeee754<T>;

[Node("Bounce InOut", "Math/Easing")]
public sealed class BounceInOutNode<T>() : EaseBase<T>(Utils.Easing.Bounce.InOut) where T : IFloatingPointIeee754<T>;