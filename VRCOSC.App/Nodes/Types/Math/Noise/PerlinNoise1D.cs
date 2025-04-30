// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Noise;

[Node("Perlin Noise 1D", "Math/Noise")]
public class PerlinNoise1DNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Position")] float position,
        [NodeValue("Seed")] int? seed,
        [NodeValue("Value")] Ref<float> outValue
    )
    {
        outValue.Value = (float)new PerlinNoise(seed).Noise(position);
    }
}