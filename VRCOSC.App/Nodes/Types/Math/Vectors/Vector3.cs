// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Math.Vectors;

[Node("Pack Vector3", "Math/Vector3")]
public sealed class PackVector3Node : ValueComputeNode<Vector3>
{
    public ValueInput<float> X = new();
    public ValueInput<float> Y = new();
    public ValueInput<float> Z = new();

    protected override Vector3 ComputeValue(IPulseContext c) => new(X.Read(c), Y.Read(c), Z.Read(c));
}

[Node("Unpack Vector3", "Math/Vector3")]
public sealed class UnpackVector3Node() : ValueConsumeNode<Vector3>("Vector")
{
    public ValueOutput<float> X = new();
    public ValueOutput<float> Y = new();
    public ValueOutput<float> Z = new();

    protected override void ConsumeValue(Vector3 vector, IPulseContext c)
    {
        X.Write(vector.X, c);
        Y.Write(vector.Y, c);
        Z.Write(vector.Z, c);
    }
}

[Node("Add", "Math/Vector3")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Plus)]
public sealed class Vector3AddNode() : SimpleResultComputeNode<Vector3>((a, b) => a + b);

[Node("Subtract", "Math/Vector3")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Minus)]
public sealed class Vector3SubtractNode() : SimpleResultComputeNode<Vector3>((a, b) => a - b);

[Node("Multiply", "Math/Vector3")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Asterisk)]
public sealed class Vector3MultiplyNode() : SimpleResultComputeNode<Vector3>((a, b) => a * b);

[Node("Transform", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3TransformNode() : SimpleResultComputeNode<Vector3, System.Numerics.Quaternion, Vector3>(Vector3.Transform);

[Node("Forward", "Math/Vector3/Units")]
[NodeCollapsed]
public sealed class Vector3UnitForwardNode() : ConstantNode<Vector3>(Vector3.UnitZ);

[Node("Backward", "Math/Vector3/Units")]
[NodeCollapsed]
public sealed class Vector3UnitBackwardNode() : ConstantNode<Vector3>(-Vector3.UnitZ);

[Node("Up", "Math/Vector3/Units")]
[NodeCollapsed]
public sealed class Vector3UnitUpNode() : ConstantNode<Vector3>(Vector3.UnitY);

[Node("Down", "Math/Vector3/Units")]
[NodeCollapsed]
public sealed class Vector3UnitDownNode() : ConstantNode<Vector3>(-Vector3.UnitY);

[Node("Right", "Math/Vector3/Units")]
[NodeCollapsed]
public sealed class Vector3UnitRightNode() : ConstantNode<Vector3>(Vector3.UnitX);

[Node("Left", "Math/Vector3/Units")]
[NodeCollapsed]
public sealed class Vector3UnitLeftNode() : ConstantNode<Vector3>(-Vector3.UnitX);

[Node("Distance", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3DistanceNode() : SimpleResultComputeNode<Vector3, float>(Vector3.Distance);

[Node("Contains", "Math/Vector3")]
public sealed class Vector3ContainsNode : Node
{
    public ValueInput<Vector3> A = new();
    public ValueInput<Vector3> B = new();
    public ValueInput<Vector3> Point = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(IPulseContext c)
    {
        Result.Write(isPointInBox(A.Read(c), B.Read(c), Point.Read(c)), c);
        return Task.CompletedTask;
    }

    private static bool isPointInBox(Vector3 a, Vector3 b, Vector3 point)
    {
        var minX = float.Min(a.X, b.X);
        var maxX = float.Max(a.X, b.X);
        var minY = float.Min(a.Y, b.Y);
        var maxY = float.Max(a.Y, b.Y);
        var minZ = float.Min(a.Z, b.Z);
        var maxZ = float.Max(a.Z, b.Z);

        return point.X >= minX && point.X <= maxX &&
               point.Y >= minY && point.Y <= maxY &&
               point.Z >= minZ && point.Z <= maxZ;
    }
}

[Node("Delta", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3DeltaNode : ValueTransformNode<Vector3>
{
    public GlobalStore<Vector3> PrevValue = new();

    protected override Vector3 TransformValue(Vector3 value, IPulseContext c)
    {
        var prevValue = PrevValue.Read(c);
        PrevValue.Write(value, c);
        return value - prevValue;
    }
}

[Node("Length", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3LengthNode() : SimpleValueTransformNode<Vector3, float>(v => v.Length());

[Node("Normalize", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3NormalizeNode() : SimpleValueTransformNode<Vector3>(Vector3.Normalize);

[Node("Dot Product", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3DotNode() : SimpleResultComputeNode<Vector3, float>(Vector3.Dot);

[Node("Cross Product", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3CrossNode() : SimpleResultComputeNode<Vector3>(Vector3.Cross);

[Node("Reflect", "Math/Vector3")]
public sealed class Vector3ReflectNode() : SimpleResultComputeNode<Vector3>(Vector3.Reflect, "Direction", "Normal");

[Node("Lerp", "Math/Vector3")]
public sealed class Vector3LerpNode : ValueComputeNode<Vector3>
{
    public ValueInput<Vector3> A = new();
    public ValueInput<Vector3> B = new();
    public ValueInput<float> T = new();

    protected override Vector3 ComputeValue(IPulseContext c) =>
        Vector3.Lerp(A.Read(c), B.Read(c), T.Read(c));
}

[Node("Slerp", "Math/Vector3")]
public sealed class Vector3SlerpNode : ValueComputeNode<Vector3>
{
    public ValueInput<Vector3> A = new();
    public ValueInput<Vector3> B = new();
    public ValueInput<float> T = new();

    protected override Vector3 ComputeValue(IPulseContext c)
    {
        var a = A.Read(c);
        var b = B.Read(c);
        var t = T.Read(c);

        var dotProduct = Vector3.Dot(a, b);
        dotProduct = float.Max(-1.0f, float.Min(1.0f, dotProduct));

        var theta = float.Acos(dotProduct) * t;
        var relativeVec = Vector3.Normalize(b - a * dotProduct);

        return (a * float.Cos(theta)) + (relativeVec * float.Sin(theta));
    }
}

[Node("Clamp Magnitude", "Math/Vector3")]
public sealed class Vector3ClampMagnitudeNode : ValueComputeNode<Vector3>
{
    public ValueInput<Vector3> Vector = new();
    public ValueInput<float> Min = new();
    public ValueInput<float> Max = new();

    protected override Vector3 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var magnitude = vec.Length();
        var min = Min.Read(c);
        var max = Max.Read(c);

        if (magnitude < min)
            return Vector3.Normalize(vec) * min;

        if (magnitude > max)
            return Vector3.Normalize(vec) * max;

        return vec;
    }
}

[Node("Clamp", "Math/Vector3")]
public sealed class Vector3ClampNode : ValueComputeNode<Vector3>
{
    public ValueInput<Vector3> Vector = new();
    public ValueInput<Vector3> Min = new();
    public ValueInput<Vector3> Max = new();

    protected override Vector3 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var min = Min.Read(c);
        var max = Max.Read(c);

        return new Vector3(
            float.Max(min.X, float.Min(max.X, vec.X)),
            float.Max(min.Y, float.Min(max.Y, vec.Y)),
            float.Max(min.Z, float.Min(max.Z, vec.Z))
        );
    }
}

[Node("Min", "Math/Vector3")]
public sealed class Vector3MinNode : ValueComputeNode<Vector3>
{
    public ValueInput<Vector3> A = new();
    public ValueInput<Vector3> B = new();

    protected override Vector3 ComputeValue(IPulseContext c)
    {
        var a = A.Read(c);
        var b = B.Read(c);
        return new Vector3(float.Min(a.X, b.X), float.Min(a.Y, b.Y), float.Min(a.Z, b.Z));
    }
}

[Node("Max", "Math/Vector3")]
public sealed class Vector3MaxNode : ValueComputeNode<Vector3>
{
    public ValueInput<Vector3> A = new();
    public ValueInput<Vector3> B = new();

    protected override Vector3 ComputeValue(IPulseContext c)
    {
        var a = A.Read(c);
        var b = B.Read(c);
        return new Vector3(float.Max(a.X, b.X), float.Max(a.Y, b.Y), float.Max(a.Z, b.Z));
    }
}

[Node("Abs", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3AbsNode() : SimpleValueTransformNode<Vector3>(v => new Vector3(float.Abs(v.X), float.Abs(v.Y), float.Abs(v.Z)));

[Node("Negate", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3NegateNode() : SimpleValueTransformNode<Vector3>(v => -v);

[Node("Angle", "Math/Vector3")]
public sealed class Vector3AngleNode : ValueComputeNode<float>
{
    public ValueInput<Vector3> From = new();
    public ValueInput<Vector3> To = new();

    protected override float ComputeValue(IPulseContext c)
    {
        var from = Vector3.Normalize(From.Read(c));
        var to = Vector3.Normalize(To.Read(c));
        var dotProduct = Vector3.Dot(from, to);
        dotProduct = float.Max(-1.0f, float.Min(1.0f, dotProduct));
        return float.Acos(dotProduct) * (180.0f / MathF.PI);
    }
}

[Node("Signed Angle", "Math/Vector3")]
public sealed class Vector3SignedAngleNode : ValueComputeNode<float>
{
    public ValueInput<Vector3> From = new();
    public ValueInput<Vector3> To = new();
    public ValueInput<Vector3> Axis = new();

    protected override float ComputeValue(IPulseContext c)
    {
        var from = From.Read(c);
        var to = To.Read(c);
        var axis = Vector3.Normalize(Axis.Read(c));

        var angle = float.Acos(float.Max(-1.0f, float.Min(1.0f, Vector3.Dot(Vector3.Normalize(from), Vector3.Normalize(to))))) * (180.0f / MathF.PI);
        var cross = Vector3.Cross(from, to);
        var sign = float.Sign(Vector3.Dot(cross, axis));

        return angle * sign;
    }
}

[Node("Distance Squared", "Math/Vector3")]
public sealed class Vector3DistanceSquaredNode : ValueComputeNode<float>
{
    public ValueInput<Vector3> A = new();
    public ValueInput<Vector3> B = new();

    protected override float ComputeValue(IPulseContext c)
    {
        var delta = A.Read(c) - B.Read(c);
        return Vector3.Dot(delta, delta);
    }
}

[Node("Round", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3RoundNode() : SimpleValueTransformNode<Vector3, Vector3>(v => new Vector3(float.Round(v.X), float.Round(v.Y), float.Round(v.Z)));

[Node("Floor", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3FloorNode() : SimpleValueTransformNode<Vector3, Vector3>(v => new Vector3(float.Floor(v.X), float.Floor(v.Y), float.Floor(v.Z)));

[Node("Ceiling", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3CeilingNode() : SimpleValueTransformNode<Vector3, Vector3>(v => new Vector3(float.Ceiling(v.X), float.Ceiling(v.Y), float.Ceiling(v.Z)));

[Node("Snap", "Math/Vector3")]
public sealed class Vector3SnapNode : ValueComputeNode<Vector3>
{
    public ValueInput<Vector3> Vector = new();
    public ValueInput<float> GridSize = new();

    protected override Vector3 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var grid = GridSize.Read(c);

        if (grid <= 0) return vec;

        return new Vector3(
            float.Round(vec.X / grid) * grid,
            float.Round(vec.Y / grid) * grid,
            float.Round(vec.Z / grid) * grid
        );
    }
}

[Node("Swing", "Math/Vector3")]
public sealed class Vector3SwingNode : ValueComputeNode<Vector3>
{
    public ValueInput<Vector3> Axis = new();
    public ValueInput<Vector3> Vector = new();
    public ValueInput<float> Angle = new();

    protected override Vector3 ComputeValue(IPulseContext c)
    {
        var axis = Vector3.Normalize(Axis.Read(c));
        var vec = Vector.Read(c);
        var angleRad = Angle.Read(c) * (MathF.PI / 180.0f);

        var cos = float.Cos(angleRad);
        var sin = float.Sin(angleRad);

        return vec * cos + Vector3.Cross(axis, vec) * sin + axis * Vector3.Dot(axis, vec) * (1 - cos);
    }
}

[Node("Project", "Math/Vector3")]
public sealed class Vector3ProjectNode : ValueComputeNode<Vector3>
{
    public ValueInput<Vector3> Vector = new();
    public ValueInput<Vector3> OnNormal = new();

    protected override Vector3 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var normal = Vector3.Normalize(OnNormal.Read(c));
        return normal * Vector3.Dot(vec, normal);
    }
}

[Node("Exclude", "Math/Vector3")]
public sealed class Vector3ExcludeNode : ValueComputeNode<Vector3>
{
    public ValueInput<Vector3> Vector = new();
    public ValueInput<Vector3> ExcludeAxis = new();

    protected override Vector3 ComputeValue(IPulseContext c)
    {
        var vec = Vector.Read(c);
        var axis = Vector3.Normalize(ExcludeAxis.Read(c));
        return vec - axis * Vector3.Dot(vec, axis);
    }
}