// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Modules.Modules.Random;

public sealed partial class RandomFloatModule : RandomModule<float>
{
    protected override void CreateAttributes()
    {
        base.CreateAttributes();
        CreateSetting(RandomFloatSetting.MinValue, "Min Value", "The minimum value of the float", 0f, 0f, 1f);
        CreateSetting(RandomFloatSetting.MaxValue, "Max Value", "The maximum value of the float", 1f, 0f, 1f);
    }

    protected override float GetRandomValue()
    {
        var min = GetSetting<float>(RandomFloatSetting.MinValue);
        var max = GetSetting<float>(RandomFloatSetting.MaxValue);
        return RandomFloat(min, max);
    }

    private enum RandomFloatSetting
    {
        MinValue,
        MaxValue
    }
}
