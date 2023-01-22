// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Modules.Random;

public sealed partial class RandomIntModule : RandomModule<int>
{
    protected override void CreateAttributes()
    {
        base.CreateAttributes();
        CreateSetting(RandomIntSetting.MinValue, "Min Value", "The minimum value of the int", 0, 0, 255);
        CreateSetting(RandomIntSetting.MaxValue, "Max Value", "The maximum value of the int", 255, 0, 255);
    }

    protected override int GetRandomValue()
    {
        var min = GetSetting<int>(RandomIntSetting.MinValue);
        var max = GetSetting<int>(RandomIntSetting.MaxValue);
        return RandomInt(min, max);
    }

    private enum RandomIntSetting
    {
        MinValue,
        MaxValue
    }
}
