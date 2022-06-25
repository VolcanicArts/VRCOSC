// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Modules.Util;

public class ModuleMaths
{
    public static float Map(float source, float sMin, float sMax, float dMin, float dMax)
    {
        return dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));
    }
}
