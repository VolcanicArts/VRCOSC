// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Utils;

public static class Interpolation
{
    public static float Lerp(float start, float final, float amount) => start + (final - start) * amount;

    public static float DampContinuously(float current, float target, float halfTimeMilli, float elapsedTimeMilli)
    {
        var exponent = elapsedTimeMilli / halfTimeMilli;
        return Lerp(current, target, 1 - MathF.Pow(0.5f, exponent));
    }

    public static float Map(float source, float sMin, float sMax, float dMin, float dMax) => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));
}