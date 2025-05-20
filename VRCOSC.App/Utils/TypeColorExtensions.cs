// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace VRCOSC.App.Utils;

public static class TypeColorExtensions
{
    private static readonly (float H, float S, float L) signed_int_base = (0f / 360f, 1f, 0.5f);
    private static readonly (float H, float S, float L) unsigned_int_base = (120f / 360f, 1f, 0.75f);
    private static readonly (float H, float S, float L) float_base = (180f / 360f, 1f, 1f);
    private static readonly (float H, float S, float L) decimal_base = (240f / 360f, 0f, 1f);
    private static readonly uint[] crc_table = generateCrcTable();

    public static SolidColorBrush GetTypeBrush(this Type type) => toBrush(GetTypeColor(type));

    public static Color GetTypeColor(this Type type)
    {
        if (type == typeof(bool)) return fromHsl(0, 0, 0.6f);
        if (type == typeof(char)) return fromHsl(30f / 360f, 37f / 85f, 7f / 85f);
        if (type == typeof(string)) return fromHsl(30f / 360f, 7f / 85f, 75f / 85f);

        if (tryGetNumericHsl(type, out var baseHsl, out var t))
        {
            if (type == typeof(decimal))
                return hslToColor(decimal_base);

            var lerped = (
                H: baseHsl.H,
                S: lerp(0.5f, 0.8f, t),
                L: lerp(0.4f, 0.7f, t)
            );
            return hslToColor(lerped);
        }

        var nameBytes = Encoding.UTF8.GetBytes(type.Name);
        var crc = nameBytes.Aggregate(0xFFFFFFFFu, (current, b) => crc_table[(current ^ b) & 0xFF] ^ current >> 8);
        crc ^= 0xFFFFFFFFu;

        var hByte = (byte)(crc >> 24);
        var sByte = (byte)(crc >> 16);
        var lByte = (byte)(crc >> 8);

        var h = hByte / 255f;
        var s = lerpUnclamped(0.25f, 0.75f, sByte / 255f);
        var l = lerpUnclamped(0.25f, 1f, lByte / 255f);

        return fromHsl(h, s, l);
    }

    private static bool tryGetNumericHsl(Type type, out (float H, float S, float L) baseHsl, out float t)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.SByte:
                baseHsl = signed_int_base;
                t = 1f / 7;
                return true;

            case TypeCode.Int16:
                baseHsl = signed_int_base;
                t = 3f / 7;
                return true;

            case TypeCode.Int32:
                baseHsl = signed_int_base;
                t = 5f / 7;
                return true;

            case TypeCode.Int64:
                baseHsl = signed_int_base;
                t = 7f / 7;
                return true;

            case TypeCode.Byte:
                baseHsl = unsigned_int_base;
                t = 1f / 8;
                return true;

            case TypeCode.UInt16:
                baseHsl = unsigned_int_base;
                t = 4f / 8;
                return true;

            case TypeCode.UInt32:
                baseHsl = unsigned_int_base;
                t = 6f / 8;
                return true;

            case TypeCode.UInt64:
                baseHsl = unsigned_int_base;
                t = 8f / 8;
                return true;

            case TypeCode.Single:
                baseHsl = float_base;
                t = 1f / 2;
                return true;

            case TypeCode.Double:
                baseHsl = float_base;
                t = 2f / 2;
                return true;

            case TypeCode.Decimal:
                baseHsl = decimal_base;
                t = 1f;
                return true;

            default:
                baseHsl = default;
                t = 0;
                return false;
        }
    }

    private static uint[] generateCrcTable()
    {
        const uint poly = 0xEDB88320u;
        var tbl = new uint[256];

        for (uint i = 0; i < 256; i++)
        {
            var c = i;

            for (var j = 0; j < 8; j++)
            {
                c = (c & 1) != 0
                    ? poly ^ c >> 1
                    : c >> 1;
            }

            tbl[i] = c;
        }

        return tbl;
    }

    private static Color hslToColor((float H, float S, float L) hsl)
    {
        double h = hsl.H, s = hsl.S, l = hsl.L;
        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            r = hueToRgb(p, q, h + 1.0 / 3);
            g = hueToRgb(p, q, h);
            b = hueToRgb(p, q, h - 1.0 / 3);
        }

        return Color.FromArgb(
            255,
            (byte)Math.Round(r * 255),
            (byte)Math.Round(g * 255),
            (byte)Math.Round(b * 255)
        );
    }

    private static double hueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2) return q;
        if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;

        return p;
    }

    private static float lerp(float a, float b, float t)
        => a + (b - a) * t;

    private static float lerpUnclamped(float a, float b, float t)
        => a + (b - a) * t;

    // Helpers to wrap System.Drawing.Color → SolidColorBrush
    private static SolidColorBrush toBrush(Color dc)
    {
        var mc = Color.FromArgb(dc.A, dc.R, dc.G, dc.B);
        var b = new SolidColorBrush(mc);
        b.Freeze();
        return b;
    }

    private static Color fromHsl(float h, float s, float l)
        => hslToColor((h, s, l));
}