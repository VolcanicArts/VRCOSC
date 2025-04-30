// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Vectors;

[Node("Distance", "Math/Vector2")]
public sealed class Vector2DistanceNode : Node
{
    [NodeProcess]
    private void process
    (
        Vector2 vectorA,
        Vector2 vectorB,
        Ref<float> outDistance
    )
    {
        outDistance.Value = Vector2.Distance(vectorA, vectorB);
    }
}