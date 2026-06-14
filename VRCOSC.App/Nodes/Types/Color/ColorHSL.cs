// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Color;

[Node("Pack ColorHSL", "Color")]
public sealed class ColorHSLPackNode() : ValueComputeNode<ColorHSL>(nameof(ColorHSL))
{
    public ValueInput<float> H = new();
    public ValueInput<float> S = new();
    public ValueInput<float> L = new();
    public ValueInput<float> A = new(defaultValue: 1f);

    protected override ColorHSL ComputeValue(IPulseContext c)
    {
        var h = float.Clamp(H.Read(c), 0f, 1f);
        var s = float.Clamp(S.Read(c), 0f, 1f);
        var l = float.Clamp(L.Read(c), 0f, 1f);
        var a = float.Clamp(A.Read(c), 0f, 1f);
        return new ColorHSL(h, s, l, a);
    }
}

[Node("Unpack ColorHSL", "Color")]
public sealed class ColorHSLUnpackNode : Node
{
    [InputMode(InputModes.Connection)]
    public ValueInput<ColorHSL> ColorHSL = new();

    public ValueOutput<float> H = new();
    public ValueOutput<float> S = new();
    public ValueOutput<float> L = new();
    public ValueOutput<float> A = new();

    protected override Task Process(IPulseContext c)
    {
        var color = ColorHSL.Read(c);
        H.Write(color.H, c);
        S.Write(color.S, c);
        L.Write(color.L, c);
        A.Write(color.A, c);
        return Task.CompletedTask;
    }
}