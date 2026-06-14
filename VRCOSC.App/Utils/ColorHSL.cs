// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace VRCOSC.App.Utils;

[StructLayout(LayoutKind.Sequential)]
public record struct ColorHSL
{
    [JsonIgnore]
    internal float _h;

    [JsonIgnore]
    internal float _s;

    [JsonIgnore]
    internal float _l;

    [JsonIgnore]
    internal float _a = 1;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public float H
    {
        readonly get => _h;
        set => _h = float.Clamp(value, 0f, 1f);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public float S
    {
        readonly get => _s;
        set => _s = float.Clamp(value, 0f, 1f);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public float L
    {
        readonly get => _l;
        set => _l = float.Clamp(value, 0f, 1f);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public float A
    {
        readonly get => _a;
        set => _a = float.Clamp(value, 0f, 1f);
    }

    public ColorHSL(float h, float s, float l, float a = 1.0f)
    {
        H = h;
        S = s;
        L = l;
        A = a;
    }

    public ColorHSL(float hsl, float a = 1.0f)
        : this(hsl, hsl, hsl, a)
    {
    }

    public ColorHSL()
        : this(0)
    {
    }

    [JsonIgnore]
    public Color AsColor => toColor(this);

    public static explicit operator Color(ColorHSL c) => c.AsColor;

    private static Color toColor(ColorHSL cHsl)
    {
        var c = (1f - float.Abs(2f * cHsl._l - 1f)) * cHsl._s;
        var x = c * (1f - float.Abs((cHsl._h / (1f / 6f)).Repeat(2f) - 1f));
        var m = cHsl._l - c / 2f;

        var cRgb = (int)(cHsl._h / (1f / 6f)) switch
        {
            0 => new Color(c, x, 0f),
            1 => new Color(x, c, 0f),
            2 => new Color(0f, c, x),
            3 => new Color(0f, x, c),
            4 => new Color(x, 0f, c),
            5 => new Color(c, 0f, x),
            _ => new Color()
        };

        return new Color(cRgb._r + m, cRgb._g + m, cRgb._b + m, cHsl._a);
    }

    public override string ToString() => $"{{{H}, {S}, {L}, {A}}}";
}