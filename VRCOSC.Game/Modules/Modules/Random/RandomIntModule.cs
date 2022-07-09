// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Random;

public class RandomIntModule : RandomModule<int>
{
    protected override void CreateAttributes()
    {
        base.CreateAttributes();
        CreateSetting(RandomIntSetting.MinValue, "Min Value", "The minimum value of the int", 0, 0, 255);
        CreateSetting(RandomIntSetting.MaxValue, "Max Value", "The maximum value of the int", 255, 0, 255);
    }

    protected override int GetRandomValue()
    {
        return ModuleMaths.RandomInt(GetSetting<int>(RandomIntSetting.MinValue), GetSetting<int>(RandomIntSetting.MaxValue));
    }

    private enum RandomIntSetting
    {
        MinValue,
        MaxValue
    }
}
