// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Noise;

[Node("Perlin Noise 3D", "Math/Noise")]
public class PerlinNoise3DNode : Node
{
    public ValueInput<Vector3> Value = new();
    public ValueInput<int?> Seed = new();
    public ValueOutput<float> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write((float)new PerlinNoise(Seed.Read(c)).Noise(Value.Read(c).X, Value.Read(c).Y, Value.Read(c).Z), c);
    }
}