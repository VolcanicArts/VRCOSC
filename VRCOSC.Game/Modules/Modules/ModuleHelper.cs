// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;

namespace VRCOSC.Game.Modules.Modules;

public static class ModuleHelper
{
    public static int[] ToDigitArray(int num, int totalWidth) => num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();

    public static float MapBetween(float value, float sMin, float sMax, float dMin, float dMax) => value / (sMax - sMin) * (dMax - dMin) + dMin;
}
