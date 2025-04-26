// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes.Types.Math;

[Node("Perlin Noise 1D", "Math/Noise")]
public class PerlinNoise1DNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Position")] float position,
        [NodeValue("Seed")] int? seed,
        [NodeValue("Value")] ref float outValue
    )
    {
        outValue = (float)new PerlinNoise(seed).Noise(position);
    }
}

[Node("Perlin Noise 2D", "Math/Noise")]
public class PerlinNoise2DNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Position")] Vector2 position,
        [NodeValue("Seed")] int? seed,
        [NodeValue("Value")] ref float outValue
    )
    {
        outValue = (float)new PerlinNoise(seed).Noise(position.X, position.Y);
    }
}

[Node("Perlin Noise 3D", "Math/Noise")]
public class PerlinNoise3DNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Position")] Vector3 position,
        [NodeValue("Seed")] int? seed,
        [NodeValue("Value")] ref float outValue
    )
    {
        outValue = (float)new PerlinNoise(seed).Noise(position.X, position.Y, position.Z);
    }
}