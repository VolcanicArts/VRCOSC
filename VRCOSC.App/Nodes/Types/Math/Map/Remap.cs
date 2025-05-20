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
        [NodeValue("In Min")] float vMin,
        [NodeValue("In Max")] float vMax,
        [NodeValue("Out Min")] float dMin,
        [NodeValue("Out Max")] float dMax,
        [NodeValue("Value")] Ref<float> outValue
    )
    {
        outValue.Value = Utils.Interpolation.Map(value, vMin, vMax, dMin, dMax);
    }
}