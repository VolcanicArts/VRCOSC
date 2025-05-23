// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Vectors;

[Node("Pack Vector3", "Math/Vector3")]
public class PackVector3Node : Node
{
    public ValueInput<float> X = new();
    public ValueInput<float> Y = new();
    public ValueInput<float> Z = new();
    public ValueOutput<Vector3> Result = new();

    protected override void Process(PulseContext c)
    {
        var x = X.Read(c);
        var y = Y.Read(c);
        var z = Z.Read(c);
        Result.Write(new Vector3(x, y, z), c);
    }
}

[Node("Distance", "Math/Vector3")]
public sealed class Vector3DistanceNode : Node
{
    public ValueInput<Vector3> VectorA = new();
    public ValueInput<Vector3> VectorB = new();
    public ValueOutput<float> Distance = new();

    protected override void Process(PulseContext c)
    {
        Distance.Write(Vector3.Distance(VectorA.Read(c), VectorB.Read(c)), c);
    }
}