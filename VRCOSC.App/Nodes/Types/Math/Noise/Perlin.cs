// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Noise;

public abstract class PerlinNoiseNode<T>(Func<PerlinNoise, T, float> func) : ValueComputeNode<float>
{
    public ValueInput<T> Value = new();
    public ValueInput<int> Seed = new();

    protected override float ComputeValue(PulseContext c) => func(new PerlinNoise(Seed.Read(c)), Value.Read(c));
}

[Node("1D", "Math/Noise/Perlin")]
public sealed class PerlinNoise1DNode() : PerlinNoiseNode<float>((n, v) => n.Noise(v))
{
    public override string DisplayName => "Perlin Noise 1D";
}

[Node("2D", "Math/Noise/Perlin")]
public sealed class PerlinNoise2DNode() : PerlinNoiseNode<Vector2>((n, v) => n.Noise(v))
{
    public override string DisplayName => "Perlin Noise 2D";
}

[Node("2D", "Math/Noise/Perlin")]
public sealed class PerlinNoise3DNode() : PerlinNoiseNode<Vector3>((n, v) => n.Noise(v))
{
    public override string DisplayName => "Perlin Noise 3D";
}