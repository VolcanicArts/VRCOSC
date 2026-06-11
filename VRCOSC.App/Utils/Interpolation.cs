// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Utils;

public static class Interpolation
{
    public static T DampContinuously<T>(T current, T target, double halfTimeMilli, double elapsedTimeMilli) where T : IFloatingPointIeee754<T>
    {
        var epsilon = T.CreateSaturating(1e-4);
        var difference = T.Abs(target - current);

        if (difference < epsilon)
            return target;

        var exponent = T.CreateSaturating(elapsedTimeMilli / halfTimeMilli);
        var result = T.Lerp(current, target, T.One - T.Pow(T.CreateSaturating(0.5d), exponent));

        return T.Abs(target - result) < epsilon ? target : result;
    }

    public static TTo Map<TFrom, TTo>(TFrom source, TFrom sMin, TFrom sMax, TTo dMin, TTo dMax) where TFrom : INumberBase<TFrom> where TTo : INumberBase<TTo>
    {
        var t = double.CreateSaturating(source - sMin) / double.CreateSaturating(sMax - sMin);
        return dMin + TTo.CreateSaturating(double.CreateSaturating(dMax - dMin) * t);
    }

    public static TTo Ease<TFrom, TTo>(TTo min, TTo max, TFrom t, EasingMode easing) where TFrom : IFloatingPointIeee754<TFrom> where TTo : INumberBase<TTo>
    {
        t = Easing.Apply(t, easing);
        var doubleT = double.CreateSaturating(t);
        var doubleMin = double.CreateSaturating(min);
        var doubleMax = double.CreateSaturating(max);
        return TTo.CreateSaturating(doubleMin + (doubleMax - doubleMin) * doubleT);
    }
}