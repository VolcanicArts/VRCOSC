using System;

namespace VRCOSC.App.Utils;

public static class Interpolation
{
    public static double Lerp(double start, double final, double amount) => start + (final - start) * amount;

    public static double DampContinuously(double current, double target, double halfTimeMilli, double elapsedTimeMilli)
    {
        var exponent = elapsedTimeMilli / halfTimeMilli;
        return Lerp(current, target, 1 - Math.Pow(0.5, exponent));
    }

    public static double Map(double source, double sMin, double sMax, double dMin, double dMax) => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));
}
