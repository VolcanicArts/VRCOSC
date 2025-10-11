// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Vectors;

[Node("Pack Vector2", "Math/Vector2")]
public sealed class PackVector2Node : Node
{
    public ValueInput<float> X = new();
    public ValueInput<float> Y = new();
    public ValueOutput<Vector2> Result = new();

    protected override Task Process(PulseContext c)
    {
        var x = X.Read(c);
        var y = Y.Read(c);
        Result.Write(new Vector2(x, y), c);
        return Task.CompletedTask;
    }
}

[Node("Unpack Vector2", "Math/Vector2")]
public sealed class UnpackVector2Node : Node
{
    public ValueInput<Vector2> Vector = new();
    public ValueOutput<float> X = new();
    public ValueOutput<float> Y = new();

    protected override Task Process(PulseContext c)
    {
        var vector = Vector.Read(c);
        X.Write(vector.X, c);
        Y.Write(vector.Y, c);
        return Task.CompletedTask;
    }
}

[Node("Distance", "Math/Vector2")]
[NodeCollapsed]
public sealed class Vector2DistanceNode : Node
{
    public ValueInput<Vector2> A = new();
    public ValueInput<Vector2> B = new();
    public ValueOutput<float> Distance = new();

    protected override Task Process(PulseContext c)
    {
        Distance.Write(Vector2.Distance(A.Read(c), B.Read(c)), c);
        return Task.CompletedTask;
    }
}

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