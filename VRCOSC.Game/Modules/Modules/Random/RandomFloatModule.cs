// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Random;

public class RandomFloatModule : RandomModule<float>
{
    protected override void CreateAttributes()
    {
        base.CreateAttributes();
        CreateSetting(RandomFloatSetting.MinValue, "Min Value", "The minimum value of the float", 0f, 0f, 1f);
        CreateSetting(RandomFloatSetting.MaxValue, "Max Value", "The maximum value of the float", 1f, 0f, 1f);
    }

    protected override float GetRandomValue()
    {
        return ModuleMaths.RandomFloat(GetSetting<float>(RandomFloatSetting.MinValue), GetSetting<float>(RandomFloatSetting.MaxValue));
    }

    private enum RandomFloatSetting
    {
        MinValue,
        MaxValue
    }
}
