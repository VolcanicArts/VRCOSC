// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Map;

[Node("Remap 0,1 To -1,1", "Math/Map")]
public class Remap0111Node : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Value")] float value,
        [NodeValue("Value")] Ref<float> outValue
    )
    {
        outValue.Value = App.Utils.Interpolation.Map(value, 0, 1, -1, 1);
    }
}