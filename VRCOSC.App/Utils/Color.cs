// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Utils;

public struct ColorHSL
{
    public int Hue;
    public float Saturation;
    public float Lightness;

    public ColorHSL(int hue, float saturation, float lightness)
    {
        Hue = hue;
        Saturation = saturation;
        Lightness = lightness;
    }

    public override string ToString() => $"{{{Hue}, {Saturation}, {Lightness}}}";
}