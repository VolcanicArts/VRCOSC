// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Color;

[Node("Pack ColorHSL", "Color")]
public sealed class ColorHSLPackNode : Node
{
    public ValueInput<int> Hue = new();
    public ValueInput<float> Saturation = new();
    public ValueInput<float> Lightness = new();

    public ValueOutput<ColorHSL> Color = new();

    protected override Task Process(PulseContext c)
    {
        var hue = Hue.Read(c) % 360;
        var saturation = float.Clamp(Saturation.Read(c), 0f, 1f);
        var lightness = float.Clamp(Lightness.Read(c), 0f, 1f);
        Color.Write(new ColorHSL(hue, saturation, lightness), c);
        return Task.CompletedTask;
    }
}

[Node("Unpack ColorHSL", "Color")]
public sealed class ColorHSLUnpackNode : Node
{
    public ValueInput<ColorHSL> Color = new();

    public ValueOutput<int> Hue = new();
    public ValueOutput<float> Saturation = new();
    public ValueOutput<float> Lightness = new();

    protected override Task Process(PulseContext c)
    {
        var colorHsl = Color.Read(c);
        Hue.Write(colorHsl.Hue, c);
        Saturation.Write(colorHsl.Saturation, c);
        Lightness.Write(colorHsl.Lightness, c);
        return Task.CompletedTask;
    }
}