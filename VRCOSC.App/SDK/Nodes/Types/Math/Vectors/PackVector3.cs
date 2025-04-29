// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.SDK.Nodes.Types.Math.Vectors;

[Node("Pack Vector3", "Math/Vector")]
public class PackVector3Node : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("X")] float x,
        [NodeValue("Y")] float y,
        [NodeValue("Z")] float z,
        [NodeValue("Vector3")] Ref<Vector3> outVector3
    )
    {
        outVector3.Value = new Vector3(x, y, z);
    }
}