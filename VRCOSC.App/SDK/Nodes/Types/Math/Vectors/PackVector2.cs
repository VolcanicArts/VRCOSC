// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.SDK.Nodes.Types.Math.Vectors;

[Node("Pack Vector2", "Math/Vector")]
public class PackVector2Node : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("X")] float x,
        [NodeValue("Y")] float y,
        [NodeValue("Vector2")] Ref<Vector2> outVector2
    )
    {
        outVector2.Value = new Vector2(x, y);
    }
}