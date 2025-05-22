// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Vectors;

[Node("Distance", "Math/Vector2")]
public sealed class Vector2DistanceNode : Node
{
    public ValueInput<Vector2> VectorA = new();
    public ValueInput<Vector2> VectorB = new();
    public ValueOutput<float> Distance = new();

    protected override void Process(PulseContext c)
    {
        Distance.Write(Vector2.Distance(VectorA.Read(c), VectorB.Read(c)), c);
    }
}