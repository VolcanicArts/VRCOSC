// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Utils;

public static class Interpolation
{
    public static T DampContinuously<T>(T current, T target, double halfTimeMilli, double elapsedTimeMilli) where T : IFloatingPointIeee754<T>
    {
        var exponent = T.CreateSaturating(elapsedTimeMilli / halfTimeMilli);
        return T.Lerp(current, target, T.One - T.Pow(T.CreateChecked(0.5), exponent));
    }

    public static T Map<T>(T source, T sMin, T sMax, T dMin, T dMax) where T : INumber<T> => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));
}