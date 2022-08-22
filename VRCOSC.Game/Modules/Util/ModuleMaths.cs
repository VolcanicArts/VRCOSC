// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.Util;

public static class ModuleMaths
{
    private static readonly Random random = new();

    private static float map(float source, float sMin, float sMax, float dMin, float dMax) => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));

    public static float RandomFloat(float min = float.MinValue, float max = float.MaxValue) => map(random.NextSingle(), 0f, 1f, min, max);

    public static int RandomInt(int min = int.MinValue, int max = int.MaxValue) => (int)RandomFloat(min, max);

    public static bool RandomBool() => (int)MathF.Round(RandomFloat(0, 1)) == 1;
}
