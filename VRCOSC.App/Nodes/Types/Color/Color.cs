// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Color;

[Node("Pack ColorHSL", "Color")]
public sealed class ColorHSLPackNode() : ValueComputeNode<ColorHSL>("Color")
{
    public ValueInput<int> Hue = new();
    public ValueInput<float> Saturation = new();
    public ValueInput<float> Lightness = new();

    protected override ColorHSL ComputeValue(IPulseContext c)
    {
        var hue = Hue.Read(c) % 360;
        var saturation = float.Clamp(Saturation.Read(c), 0f, 1f);
        var lightness = float.Clamp(Lightness.Read(c), 0f, 1f);
        return new ColorHSL(hue, saturation, lightness);
    }
}

[Node("Unpack ColorHSL", "Color")]
public sealed class ColorHSLUnpackNode() : ValueConsumeNode<ColorHSL>("Color")
{
    public ValueOutput<int> Hue = new();
    public ValueOutput<float> Saturation = new();
    public ValueOutput<float> Lightness = new();

    protected override void ConsumeValue(ColorHSL color, IPulseContext c)
    {
        Hue.Write(color.Hue, c);
        Saturation.Write(color.Saturation, c);
        Lightness.Write(color.Lightness, c);
    }
}