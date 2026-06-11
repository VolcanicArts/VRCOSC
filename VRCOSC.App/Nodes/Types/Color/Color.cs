// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Color;

[Node("Pack Color", "Color")]
public sealed class ColorPackNode() : ValueComputeNode<Utils.Color>(nameof(Utils.Color))
{
    public ValueInput<float> R = new();
    public ValueInput<float> G = new();
    public ValueInput<float> B = new();
    public ValueInput<float> A = new(defaultValue: 1f);

    protected override Utils.Color ComputeValue(IPulseContext c)
    {
        var r = float.Clamp(R.Read(c), 0f, 1f);
        var g = float.Clamp(G.Read(c), 0f, 1f);
        var b = float.Clamp(B.Read(c), 0f, 1f);
        var a = float.Clamp(A.Read(c), 0f, 1f);
        return new Utils.Color(r, g, b, a);
    }
}

[Node("Unpack Color", "Color")]
public sealed class ColorUnpackNode : Node
{
    [InputMode(InputModes.Connection)]
    public ValueInput<Utils.Color> Color = new();

    public ValueOutput<float> R = new();
    public ValueOutput<float> G = new();
    public ValueOutput<float> B = new();
    public ValueOutput<float> A = new();

    protected override Task Process(IPulseContext c)
    {
        var color = Color.Read(c);
        R.Write(color.R, c);
        G.Write(color.G, c);
        B.Write(color.B, c);
        A.Write(color.A, c);
        return Task.CompletedTask;
    }
}