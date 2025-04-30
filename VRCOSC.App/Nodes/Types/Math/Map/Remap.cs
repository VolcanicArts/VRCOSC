// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Map;

[Node("Remap", "Math/Map")]
public class RemapNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Value")] float value,
        [NodeValue("Value Min")] float vMin,
        [NodeValue("Value Max")] float vMax,
        [NodeValue("To Min")] float dMin,
        [NodeValue("To Max")] float dMax,
        [NodeValue("Value")] Ref<float> outValue
    )
    {
        outValue.Value = App.Utils.Interpolation.Map(value, vMin, vMax, dMin, dMax);
    }
}