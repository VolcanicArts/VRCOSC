// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Vectors;

[Node("Pack Vector3", "Math/Vector3")]
public sealed class PackVector3Node : ValueComputeNode<Vector3>
{
    public ValueInput<float> X = new();
    public ValueInput<float> Y = new();
    public ValueInput<float> Z = new();

    protected override Vector3 ComputeValue(PulseContext c) => new(X.Read(c), Y.Read(c), Z.Read(c));
}

[Node("Unpack Vector3", "Math/Vector3")]
public sealed class UnpackVector3Node() : ValueConsumeNode<Vector3>("Vector")
{
    public ValueOutput<float> X = new();
    public ValueOutput<float> Y = new();
    public ValueOutput<float> Z = new();

    protected override void ConsumeValue(Vector3 vector, PulseContext c)
    {
        X.Write(vector.X, c);
        Y.Write(vector.Y, c);
        Z.Write(vector.Z, c);
    }
}

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

    protected override Task Process(PulseContext c)
    {
        Result.Write(isPointInBox(A.Read(c), B.Read(c), Point.Read(c)), c);
        return Task.CompletedTask;
    }

    private static bool isPointInBox(Vector3 a, Vector3 b, Vector3 point)
    {
        var minX = MathF.Min(a.X, b.X);
        var maxX = MathF.Max(a.X, b.X);
        var minY = MathF.Min(a.Y, b.Y);
        var maxY = MathF.Max(a.Y, b.Y);
        var minZ = MathF.Min(a.Z, b.Z);
        var maxZ = MathF.Max(a.Z, b.Z);

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

    protected override Vector3 TransformValue(Vector3 value, PulseContext c)
    {
        var prevValue = PrevValue.Read(c);
        PrevValue.Write(value, c);
        return value - prevValue;
    }
}

[Node("Length", "Math/Vector3")]
[NodeCollapsed]
public sealed class Vector3LengthNode() : SimpleValueTransformNode<Vector3, float>(v => v.Length());