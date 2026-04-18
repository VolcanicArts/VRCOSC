// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Numerics;

namespace VRCOSC.App.Utils;

public class PerlinNoise
{
    private static readonly Vector3[] gradients = new Vector3[]
    {
        new(1, 1, 0), new(-1, 1, 0),
        new(1, -1, 0), new(-1, -1, 0),
        new(1, 0, 1), new(-1, 0, 1),
        new(1, 0, -1), new(-1, 0, -1),
        new(0, 1, 1), new(0, -1, 1),
        new(0, 1, -1), new(0, -1, -1)
    };

    private static readonly int[] default_perm = new[]
    {
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
        140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
        247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
        57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
        74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
        60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
        65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
        200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
        52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
        207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
        119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
        129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
        218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
        81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
        184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
        222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
        // repeat
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
        140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
        247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
        57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
        74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
        60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
        65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
        200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
        52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
        207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
        119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
        129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
        218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
        81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
        184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
        222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    };

    private readonly int[] _pt;

    public PerlinNoise(int seed = 0)
    {
        var rnd = new Random(seed);
        var p = Enumerable.Range(0, 256).ToArray();

        for (var i = p.Length - 1; i > 0; i--)
        {
            var j = rnd.Next(i + 1);
            (p[i], p[j]) = (p[j], p[i]);
        }

        _pt = p.Concat(p).ToArray();
    }

    public float Noise(float value) => Noise(new Vector3(value, 0f, 0f));

    public float Noise(Vector2 value) => Noise(new Vector3(value.X, value.Y, 0f));

    public float Noise(Vector3 value)
    {
        var fx = float.Floor(value.X);
        var fy = float.Floor(value.Y);
        var fz = float.Floor(value.Z);

        var xi = int.CreateChecked(fx) & 255;
        var yi = int.CreateChecked(fy) & 255;
        var zi = int.CreateChecked(fz) & 255;

        var xf = value.X - fx;
        var yf = value.Y - fy;
        var zf = value.Z - fz;

        var u = smooth(xf);
        var v = smooth(yf);
        var w = smooth(zf);

        var aaa = _pt[_pt[_pt[xi] + yi] + zi];
        var aba = _pt[_pt[_pt[xi] + yi + 1] + zi];
        var aab = _pt[_pt[_pt[xi] + yi] + zi + 1];
        var abb = _pt[_pt[_pt[xi] + yi + 1] + zi + 1];
        var baa = _pt[_pt[_pt[xi + 1] + yi] + zi];
        var bba = _pt[_pt[_pt[xi + 1] + yi + 1] + zi];
        var bab = _pt[_pt[_pt[xi + 1] + yi] + zi + 1];
        var bbb = _pt[_pt[_pt[xi + 1] + yi + 1] + zi + 1];

        const float one = 1f;

        var x1 = float.Lerp(gradDot(aaa, xf, yf, zf), gradDot(baa, xf - one, yf, zf), u);
        var x2 = float.Lerp(gradDot(aba, xf, yf - one, zf), gradDot(bba, xf - one, yf - one, zf), u);
        var y1 = float.Lerp(x1, x2, v);
        var x3 = float.Lerp(gradDot(aab, xf, yf, zf - one), gradDot(bab, xf - one, yf, zf - one), u);
        var x4 = float.Lerp(gradDot(abb, xf, yf - one, zf - one), gradDot(bbb, xf - one, yf - one, zf - one), u);
        var y2 = float.Lerp(x3, x4, v);

        return float.Lerp(y1, y2, w);
    }

    private static float gradDot(int hash, float x, float y, float z)
    {
        var g = gradients[hash % gradients.Length];
        return g.X * x + g.Y * y + g.Z * z;
    }

    private static float smooth(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);
}