// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Math.Noise;

[Node("Perlin Noise 1D", "Math/Noise")]
public class PerlinNoise1DNode : Node
{
    public ValueInput<float> Value = new();
    public ValueInput<int?> Seed = new();
    public ValueOutput<float> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write((float)new PerlinNoise(Seed.Read(c)).Noise(Value.Read(c)), c);
        return Task.CompletedTask;
    }
}