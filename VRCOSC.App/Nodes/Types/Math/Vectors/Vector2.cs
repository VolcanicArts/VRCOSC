// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Math.Vectors;

[Node("Pack Vector2", "Math/Vector2")]
public sealed class PackVector2Node : ValueComputeNode<Vector2>
{
    public ValueInput<float> X = new();
    public ValueInput<float> Y = new();

    protected override Vector2 ComputeValue(IPulseContext c) => new(X.Read(c), Y.Read(c));
}

[Node("Unpack Vector2", "Math/Vector2")]
public sealed class UnpackVector2Node() : ValueConsumeNode<Vector2>("Vector")
{
    public ValueOutput<float> X = new();
    public ValueOutput<float> Y = new();

    protected override void ConsumeValue(Vector2 vector, IPulseContext c)
    {
        X.Write(vector.X, c);
        Y.Write(vector.Y, c);
    }
}

[Node("Add", "Math/Vector2")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Plus)]
public sealed class Vector2AddNode() : SimpleResultComputeNode<Vector2>((a, b) => a + b);

[Node("Subtract", "Math/Vector2")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Minus)]
public sealed class Vector2SubtractNode() : SimpleResultComputeNode<Vector2>((a, b) => a - b);

[Node("Multiply", "Math/Vector2")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Asterisk)]
public sealed class Vector2MultiplyNode() : SimpleResultComputeNode<Vector2>((a, b) => a * b);

[Node("Transform", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2TransformNode() : SimpleResultComputeNode<Vector2, System.Numerics.Quaternion, Vector2>(Vector2.Transform);

[Node("Up", "Math/Vector2/Units")]
[NodeCollapsed]
public sealed class Vector2UnitUpNode() : ConstantNode<Vector2>(Vector2.UnitY);

[Node("Down", "Math/Vector2/Units")]
[NodeCollapsed]
public sealed class Vector2UnitDownNode() : ConstantNode<Vector2>(-Vector2.UnitY);

[Node("Right", "Math/Vector2/Units")]
[NodeCollapsed]
public sealed class Vector2UnitRightNode() : ConstantNode<Vector2>(Vector2.UnitX);

[Node("Left", "Math/Vector2/Units")]
[NodeCollapsed]
public sealed class Vector2UnitLeftNode() : ConstantNode<Vector2>(-Vector2.UnitX);

[Node("Distance", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2DistanceNode() : SimpleResultComputeNode<Vector2, float>(Vector2.Distance);

[Node("Contains", "Math/Vector2")]
public sealed class Vector2ContainsNode : Node
{
    public ValueInput<Vector2> A = new();
    public ValueInput<Vector2> B = new();
    public ValueInput<Vector2> Point = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(IPulseContext c)
    {
        Result.Write(isPointInBox(A.Read(c), B.Read(c), Point.Read(c)), c);
        return Task.CompletedTask;
    }

    private static bool isPointInBox(Vector2 a, Vector2 b, Vector2 point)
    {
        var minX = MathF.Min(a.X, b.X);
        var maxX = MathF.Max(a.X, b.X);
        var minY = MathF.Min(a.Y, b.Y);
        var maxY = MathF.Max(a.Y, b.Y);

        return point.X >= minX && point.X <= maxX &&
               point.Y >= minY && point.Y <= maxY;
    }
}

[Node("Delta", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2DeltaNode : ValueTransformNode<Vector2>
{
    public GlobalStore<Vector2> PrevValue = new();

    protected override Task Process(IPulseContext c)
    {
        return Task.CompletedTask;
    }

    protected override Vector2 TransformValue(Vector2 value, IPulseContext c)
    {
        var prevValue = PrevValue.Read(c);
        PrevValue.Write(value, c);
        return value - prevValue;
    }
}

[Node("Length", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2LengthNode() : SimpleValueTransformNode<Vector2, float>(v => v.Length());

[Node("Normalize", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2NormalizeNode() : SimpleValueTransformNode<Vector2>(Vector2.Normalize);

[Node("Dot Product", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2DotNode() : SimpleResultComputeNode<Vector2, float>(Vector2.Dot);

[Node("Cross Product", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2CrossNode() : SimpleResultComputeNode<Vector2, float>(Vector2.Cross);

[Node("Reflect", "Math/Vector2")]
public sealed class Vector2ReflectNode() : SimpleResultComputeNode<Vector2>(Vector2.Reflect, "Direction", "Normal");

[Node("Lerp", "Math/Vector2")]
public sealed class Vector2LerpNode : ValueComputeNode<Vector2>
{
    public ValueInput<Vector2> A = new();
    public ValueInput<Vector2> B = new();
    public ValueInput<float> T = new();

    protected override Vector2 ComputeValue(IPulseContext c) =>
        Vector2.Lerp(A.Read(c), B.Read(c), T.Read(c));
}

[Node("Slerp", "Math/Vector2")]
public sealed class Vector2SlerpNode : ValueComputeNode<Vector2>
{
    public ValueInput<Vector2> A = new();
    public ValueInput<Vector2> B = new();
    public ValueInput<float> T = new();

    protected override Vector2 ComputeValue(IPulseContext c)
    {
        var a = A.Read(c);
        var b = B.Read(c);
        var t = T.Read(c);

        var dotProduct = Vector2.Dot(a, b);
        dotProduct = float.Max(-1.0f, float.Min(1.0f, dotProduct));

        var theta = float.Acos(dotProduct) * t;
        var relativeVec = Vector2.Normalize(b - a * dotProduct);

        return (a * float.Cos(theta)) + (relativeVec * float.Sin(theta));
    }
}

[Node("Clamp Magnitude", "Math/Vector2")]
public sealed class Vector2ClampMagnitudeNode : ValueComputeNode<Vector2>
{
    public ValueInput<Vector2> Vector = new();
    public ValueInput<float> Min = new();
    public ValueInput<float> Max = new();

    protected override Vector2 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var magnitude = vec.Length();
        var min = Min.Read(c);
        var max = Max.Read(c);

        if (magnitude < min)
            return Vector2.Normalize(vec) * min;

        if (magnitude > max)
            return Vector2.Normalize(vec) * max;

        return vec;
    }
}

[Node("Clamp", "Math/Vector2")]
public sealed class Vector2ClampNode : ValueComputeNode<Vector2>
{
    public ValueInput<Vector2> Vector = new();
    public ValueInput<Vector2> Min = new();
    public ValueInput<Vector2> Max = new();

    protected override Vector2 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var min = Min.Read(c);
        var max = Max.Read(c);

        return new Vector2(
            float.Max(min.X, float.Min(max.X, vec.X)),
            float.Max(min.Y, float.Min(max.Y, vec.Y))
        );
    }
}

[Node("Min", "Math/Vector2")]
public sealed class Vector2MinNode : ValueComputeNode<Vector2>
{
    public ValueInput<Vector2> A = new();
    public ValueInput<Vector2> B = new();

    protected override Vector2 ComputeValue(IPulseContext c)
    {
        var a = A.Read(c);
        var b = B.Read(c);
        return new Vector2(float.Min(a.X, b.X), float.Min(a.Y, b.Y));
    }
}

[Node("Max", "Math/Vector2")]
public sealed class Vector2MaxNode : ValueComputeNode<Vector2>
{
    public ValueInput<Vector2> A = new();
    public ValueInput<Vector2> B = new();

    protected override Vector2 ComputeValue(IPulseContext c)
    {
        var a = A.Read(c);
        var b = B.Read(c);
        return new Vector2(float.Max(a.X, b.X), float.Max(a.Y, b.Y));
    }
}

[Node("Abs", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2AbsNode() : SimpleValueTransformNode<Vector2>(v => new Vector2(float.Abs(v.X), float.Abs(v.Y)));

[Node("Negate", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2NegateNode() : SimpleValueTransformNode<Vector2>(v => -v);

[Node("Angle", "Math/Vector2")]
public sealed class Vector2AngleNode : ValueComputeNode<float>
{
    public ValueInput<Vector2> From = new();
    public ValueInput<Vector2> To = new();

    protected override float ComputeValue(IPulseContext c)
    {
        var from = Vector2.Normalize(From.Read(c));
        var to = Vector2.Normalize(To.Read(c));
        var dotProduct = Vector2.Dot(from, to);
        dotProduct = float.Max(-1.0f, float.Min(1.0f, dotProduct));
        return float.Acos(dotProduct) * (180.0f / MathF.PI);
    }
}

[Node("Signed Angle", "Math/Vector2")]
public sealed class Vector2SignedAngleNode : ValueComputeNode<float>
{
    public ValueInput<Vector2> From = new();
    public ValueInput<Vector2> To = new();

    protected override float ComputeValue(IPulseContext c)
    {
        var from = From.Read(c);
        var to = To.Read(c);

        var angle = float.Acos(float.Max(-1.0f, float.Min(1.0f, Vector2.Dot(Vector2.Normalize(from), Vector2.Normalize(to))))) * (180.0f / MathF.PI);
        var cross = Vector2.Cross(from, to);
        var sign = float.Sign(cross);

        return angle * sign;
    }
}

[Node("Distance Squared", "Math/Vector2")]
public sealed class Vector2DistanceSquaredNode : ValueComputeNode<float>
{
    public ValueInput<Vector2> A = new();
    public ValueInput<Vector2> B = new();

    protected override float ComputeValue(IPulseContext c)
    {
        var delta = A.Read(c) - B.Read(c);
        return Vector2.Dot(delta, delta);
    }
}

[Node("Round", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2RoundNode() : SimpleValueTransformNode<Vector2, Vector2>(v => new Vector2(float.Round(v.X), float.Round(v.Y)));

[Node("Floor", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2FloorNode() : SimpleValueTransformNode<Vector2, Vector2>(v => new Vector2(float.Floor(v.X), float.Floor(v.Y)));

[Node("Ceiling", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2CeilingNode() : SimpleValueTransformNode<Vector2, Vector2>(v => new Vector2(float.Ceiling(v.X), float.Ceiling(v.Y)));

[Node("Snap", "Math/Vector2")]
public sealed class Vector2SnapNode : ValueComputeNode<Vector2>
{
    public ValueInput<Vector2> Vector = new();
    public ValueInput<float> GridSize = new();

    protected override Vector2 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var grid = GridSize.Read(c);

        if (grid <= 0) return vec;

        return new Vector2(
            float.Round(vec.X / grid) * grid,
            float.Round(vec.Y / grid) * grid
        );
    }
}

[Node("Rotate", "Math/Vector2")]
public sealed class Vector2RotateNode : ValueComputeNode<Vector2>
{
    public ValueInput<Vector2> Vector = new();
    public ValueInput<float> Angle = new();

    protected override Vector2 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var angleRad = Angle.Read(c) * (MathF.PI / 180.0f);

        var cos = float.Cos(angleRad);
        var sin = float.Sin(angleRad);

        return new Vector2(
            vec.X * cos - vec.Y * sin,
            vec.X * sin + vec.Y * cos
        );
    }
}

[Node("Project", "Math/Vector2")]
public sealed class Vector2ProjectNode : ValueComputeNode<Vector2>
{
    public ValueInput<Vector2> Vector = new();
    public ValueInput<Vector2> OnNormal = new();

    protected override Vector2 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var normal = Vector2.Normalize(OnNormal.Read(c));
        return normal * Vector2.Dot(vec, normal);
    }
}

[Node("Exclude", "Math/Vector2")]
public sealed class Vector2ExcludeNode : ValueComputeNode<Vector2>
{
    public ValueInput<Vector2> Vector = new();
    public ValueInput<Vector2> ExcludeAxis = new();

    protected override Vector2 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var axis = Vector2.Normalize(ExcludeAxis.Read(c));
        return vec - axis * Vector2.Dot(vec, axis);
    }
}