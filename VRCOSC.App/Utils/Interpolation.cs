// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Utils;

public static class Interpolation
{
    public static T DampContinuously<T>(T current, T target, double halfTimeMilli, double elapsedTimeMilli) where T : IFloatingPointIeee754<T>
    {
        var exponent = T.CreateSaturating(elapsedTimeMilli / halfTimeMilli);
        return T.Lerp(current, target, T.One - T.Pow(T.CreateSaturating(0.5d), exponent));
    }

    public static TTo Map<TFrom, TTo>(TFrom source, TFrom sMin, TFrom sMax, TTo dMin, TTo dMax) where TFrom : INumberBase<TFrom> where TTo : INumberBase<TTo>
    {
        var t = double.CreateSaturating(source - sMin) / double.CreateSaturating(sMax - sMin);
        return dMin + TTo.CreateSaturating(double.CreateSaturating(dMax - dMin) * t);
    }
}