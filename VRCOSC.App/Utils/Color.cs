// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace VRCOSC.App.Utils;

[StructLayout(LayoutKind.Sequential)]
public record struct Color
{
    [JsonIgnore]
    internal float _r;

    [JsonIgnore]
    internal float _g;

    [JsonIgnore]
    internal float _b;

    [JsonIgnore]
    internal float _a = 1;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public float R
    {
        readonly get => _r;
        set => _r = float.Clamp(value, 0f, 1f);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public float G
    {
        readonly get => _g;
        set => _g = float.Clamp(value, 0f, 1f);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public float B
    {
        readonly get => _b;
        set => _b = float.Clamp(value, 0f, 1f);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public float A
    {
        readonly get => _a;
        set => _a = float.Clamp(value, 0f, 1f);
    }

    public Color(float r, float g, float b, float a = 1.0f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(float rgb, float a = 1.0f)
        : this(rgb, rgb, rgb, a)
    {
    }

    public Color(System.Windows.Media.Color color)
        : this(color.ScR, color.ScG, color.ScB, color.ScA)
    {
    }

    public Color()
        : this(0)
    {
    }

    [JsonIgnore]
    public ColorHSL AsColorHSL => toColorHSL(this);

    public static Color Black => new(0);

    public static explicit operator ColorHSL(Color c) => c.AsColorHSL;

    private static ColorHSL toColorHSL(Color c)
    {
        var cHsl = new ColorHSL();

        var cmax = float.Max(c._r, float.Max(c._g, c._b));
        var cmin = float.Min(c._r, float.Min(c._g, c._b));
        var delta = cmax - cmin;

        if (delta == 0f)
            cHsl._h = 0f;
        else if (float.Abs(cmax - c._r) < float.Epsilon)
            cHsl._h = ((c._g - c._b) / delta).Repeat(6f) / 6f;
        else if (float.Abs(cmax - c._g) < float.Epsilon)
            cHsl._h = ((c._b - c._r) / delta + 2f) / 6f;
        else
            cHsl._h = ((c._r - c._g) / delta + 4f) / 6f;

        cHsl._l = (cmax + cmin) / 2f;
        cHsl._s = delta == 0f ? 0f : delta / (1f - float.Abs(2f * cHsl._l - 1f));
        cHsl._a = c._a;
        return cHsl;
    }

    public override string ToString() => $"{{{R}, {G}, {B}, {A}}}";
}