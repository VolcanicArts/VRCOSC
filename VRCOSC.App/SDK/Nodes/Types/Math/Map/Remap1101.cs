// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Map;

[Node("Remap -1,1 To 0,1", "Math/Map")]
public class Remap1101Node : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Value")] float value,
        [NodeValue("Value")] Ref<float> outValue
    )
    {
        outValue.Value = App.Utils.Interpolation.Map(value, -1, 1, 0, 1);
    }
}