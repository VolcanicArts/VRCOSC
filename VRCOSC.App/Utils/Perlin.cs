// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Numerics;

namespace VRCOSC.App.Utils;

public class PerlinNoise
{
    // Classic 12-vector gradient set
    private static readonly Vector3[] gradients = new[]
    {
        new Vector3(1, 1, 0), new Vector3(-1, 1, 0),
        new Vector3(1, -1, 0), new Vector3(-1, -1, 0),
        new Vector3(1, 0, 1), new Vector3(-1, 0, 1),
        new Vector3(1, 0, -1), new Vector3(-1, 0, -1),
        new Vector3(0, 1, 1), new Vector3(0, -1, 1),
        new Vector3(0, 1, -1), new Vector3(0, -1, -1),
    };

    // Default 512-entry permutation table (two copies of 0–255)
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
    private readonly Func<double, double> _smooth;

    /// <summary>
    /// Create a PerlinNoise generator.
    /// </summary>
    /// <param name="seed">
    /// If non-null, used to seed a shuffle of 0..255; the resulting table is duplicated to 512 entries.
    /// If null, uses the built-in classic table.
    /// </param>
    /// <param name="smoothingFunction">
    /// Optional fade curve (defaults to 6t⁵–15t⁴+10t³).
    /// </param>
    public PerlinNoise(int? seed = null, Func<double, double>? smoothingFunction = null)
    {
        _smooth = smoothingFunction ?? smoothToSCurve;

        if (seed.HasValue)
        {
            // create a reproducible 0..255 sequence
            var rnd = new Random(seed.Value);
            var p = Enumerable.Range(0, 256).ToArray();

            // Fisher–Yates shuffle
            for (var i = p.Length - 1; i > 0; i--)
            {
                var j = rnd.Next(i + 1);
                (p[i], p[j]) = (p[j], p[i]);
            }

            // duplicate to 512
            _pt = p.Concat(p).ToArray();
        }
        else
        {
            _pt = default_perm;
        }
    }

    public double Noise(double x, double y = 0d, double z = 0d)
    {
        // --- FIND UNIT CUBE CORNERS ---
        int xi = (int)Math.Floor(x) & 255;
        int yi = (int)Math.Floor(y) & 255;
        int zi = (int)Math.Floor(z) & 255;

        // --- RELATIVE COORDINATES IN CUBE ---
        double xf = x - Math.Floor(x);
        double yf = y - Math.Floor(y);
        double zf = z - Math.Floor(z);

        // --- FADE (S-CURVE) ---
        double u = _smooth(xf);
        double v = _smooth(yf);
        double w = _smooth(zf);

        // --- PERMUTE THREE TIMES FOR EACH CORNER ---
        int aaa = _pt[_pt[_pt[xi] + yi] + zi];
        int aba = _pt[_pt[_pt[xi] + yi + 1] + zi];
        int aab = _pt[_pt[_pt[xi] + yi] + zi + 1];
        int abb = _pt[_pt[_pt[xi] + yi + 1] + zi + 1];
        int baa = _pt[_pt[_pt[xi + 1] + yi] + zi];
        int bba = _pt[_pt[_pt[xi + 1] + yi + 1] + zi];
        int bab = _pt[_pt[_pt[xi + 1] + yi] + zi + 1];
        int bbb = _pt[_pt[_pt[xi + 1] + yi + 1] + zi + 1];

        // --- GRADIENT DOTS & INTERPOLATE ---
        double x1 = Lerp(GradDot(aaa, xf, yf, zf),
            GradDot(baa, xf - 1, yf, zf), u);

        double x2 = Lerp(GradDot(aba, xf, yf - 1, zf),
            GradDot(bba, xf - 1, yf - 1, zf), u);
        double y1 = Lerp(x1, x2, v);

        double x3 = Lerp(GradDot(aab, xf, yf, zf - 1),
            GradDot(bab, xf - 1, yf, zf - 1), u);

        double x4 = Lerp(GradDot(abb, xf, yf - 1, zf - 1),
            GradDot(bbb, xf - 1, yf - 1, zf - 1), u);
        double y2 = Lerp(x3, x4, v);

        return Lerp(y1, y2, w);
    }

    public double NoiseTiled(double x, double y = 0, double z = 0, int tileRegion = 2)
    {
        // --- COMPUTE FLOOR AND WRAP AT tileRegion ---
        int fx = (int)Math.Floor(x), fy = (int)Math.Floor(y), fz = (int)Math.Floor(z);
        int xi0 = (fx % tileRegion + tileRegion) % tileRegion;
        int yi0 = (fy % tileRegion + tileRegion) % tileRegion;
        int zi0 = (fz % tileRegion + tileRegion) % tileRegion;
        int xi1 = (xi0 + 1) % tileRegion;
        int yi1 = (yi0 + 1) % tileRegion;
        int zi1 = (zi0 + 1) % tileRegion;

        // --- RELATIVE FRACTIONS ---
        double xf = x - fx;
        double yf = y - fy;
        double zf = z - fz;

        // --- FADE ---
        double u = _smooth(xf);
        double v = _smooth(yf);
        double w = _smooth(zf);

        // --- PERMUTE (3 deep) FOR TILED COORDS ---
        int aaa = _pt[_pt[_pt[xi0] + yi0] + zi0];
        int aba = _pt[_pt[_pt[xi0] + yi1] + zi0];
        int aab = _pt[_pt[_pt[xi0] + yi0] + zi1];
        int abb = _pt[_pt[_pt[xi0] + yi1] + zi1];
        int baa = _pt[_pt[_pt[xi1] + yi0] + zi0];
        int bba = _pt[_pt[_pt[xi1] + yi1] + zi0];
        int bab = _pt[_pt[_pt[xi1] + yi0] + zi1];
        int bbb = _pt[_pt[_pt[xi1] + yi1] + zi1];

        // --- DOT + INTERPOLATE ---
        double x1 = Lerp(GradDot(aaa, xf, yf, zf),
            GradDot(baa, xf - 1, yf, zf), u);

        double x2 = Lerp(GradDot(aba, xf, yf - 1, zf),
            GradDot(bba, xf - 1, yf - 1, zf), u);
        double y1 = Lerp(x1, x2, v);

        double x3 = Lerp(GradDot(aab, xf, yf, zf - 1),
            GradDot(bab, xf - 1, yf, zf - 1), u);

        double x4 = Lerp(GradDot(abb, xf, yf - 1, zf - 1),
            GradDot(bbb, xf - 1, yf - 1, zf - 1), u);
        double y2 = Lerp(x3, x4, v);

        return Lerp(y1, y2, w);
    }

    // Helper methods (make sure you still have these in your class):
    private static double Lerp(double a, double b, double t) => a + t * (b - a);

    private double GradDot(int hash, double x, double y, double z)
    {
        // safe modulo into the 12-gradient table
        var g = gradients[hash % gradients.Length];
        return g.X * x + g.Y * y + g.Z * z;
    }

    private static double smoothToSCurve(double t) => t * t * t * (t * (t * 6 - 15) + 10);

    private static int mod(int x, int m) => (x % m + m) % m;
}