// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Map;

[Node("Remap 0,1 To -1,1", "Math/Map")]
public class Remap0111Node : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float value,
        [NodeValue] Ref<float> outValue
    )
    {
        outValue.Value = Utils.Interpolation.Map(value, 0, 1, -1, 1);
    }
}