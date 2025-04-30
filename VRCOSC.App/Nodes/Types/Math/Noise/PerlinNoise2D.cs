// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Noise;

[Node("Perlin Noise 2D", "Math/Noise")]
public class PerlinNoise2DNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Position")] Vector2 position,
        [NodeValue("Seed")] int? seed,
        [NodeValue("Value")] Ref<float> outValue
    )
    {
        outValue.Value = (float)new PerlinNoise(seed).Noise(position.X, position.Y);
    }
}