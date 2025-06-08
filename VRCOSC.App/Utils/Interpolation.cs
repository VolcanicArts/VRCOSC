// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Utils;

public static class Interpolation
{
    public static double Lerp(double start, double final, double amount) => start + (final - start) * amount;

    public static double DampContinuously(double current, double target, float halfTimeMilli, float elapsedTimeMilli)
    {
        var exponent = elapsedTimeMilli / halfTimeMilli;
        return Lerp(current, target, 1 - MathF.Pow(0.5f, exponent));
    }

    public static double Map(double source, double sMin, double sMax, double dMin, double dMax) => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));
}