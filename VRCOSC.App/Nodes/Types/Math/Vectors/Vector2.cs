// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Vectors;

[Node("Pack Vector2", "Math/Vector2")]
public sealed class PackVector2Node : ValueComputeNode<Vector2>
{
    public ValueInput<float> X = new();
    public ValueInput<float> Y = new();

    protected override Vector2 ComputeValue(PulseContext c) => new(X.Read(c), Y.Read(c));
}

[Node("Unpack Vector2", "Math/Vector2")]
public sealed class UnpackVector2Node() : ValueConsumeNode<Vector2>("Vector")
{
    public ValueOutput<float> X = new();
    public ValueOutput<float> Y = new();

    protected override void ConsumeValue(Vector2 vector, PulseContext c)
    {
        X.Write(vector.X, c);
        Y.Write(vector.Y, c);
    }
}

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

    protected override Task Process(PulseContext c)
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

    protected override Task Process(PulseContext c)
    {
        return Task.CompletedTask;
    }

    protected override Vector2 TransformValue(Vector2 value, PulseContext c)
    {
        var prevValue = PrevValue.Read(c);
        PrevValue.Write(value, c);
        return value - prevValue;
    }
}

[Node("Length", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2LengthNode() : SimpleValueTransformNode<Vector2, float>(v => v.Length());