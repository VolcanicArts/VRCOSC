// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Random;

public class RandomIntModule : Module
{
    public override string Title => "Random Int";
    public override string Description => "Sends a random int over a variable time period";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Colour4.Coral.Darken(0.5f);
    public override ModuleType ModuleType => ModuleType.General;
    protected override double DeltaUpdate => GetSetting<int>(RandomIntSetting.DeltaUpdate);

    private readonly System.Random random = new();

    public override void CreateAttributes()
    {
        CreateSetting(RandomIntSetting.DeltaUpdate, "Time Between Update", "The amount of time, in milliseconds, between each random value", 1000);
        CreateSetting(RandomIntSetting.MinValue, "Min Value", "The minimum value of the int. VRChat's minimum is 0", 0);
        CreateSetting(RandomIntSetting.MaxValue, "Max Value", "The maximum value of the int. VRChat's maximum is 255", 255);

        CreateOutputParameter(RandomIntOutputParameter.RandomInt, "Random Int", "A random int value", "/avatar/parameters/RandomInt");
    }

    protected override void OnUpdate()
    {
        float randomFloat = (float)random.NextDouble();
        int randomIntMapped = (int)ModuleMaths.Map(randomFloat, 0, 1, GetSetting<int>(RandomIntSetting.MinValue), GetSetting<int>(RandomIntSetting.MaxValue));
        SendParameter(RandomIntOutputParameter.RandomInt, randomIntMapped);
    }

    private enum RandomIntSetting
    {
        DeltaUpdate,
        MinValue,
        MaxValue
    }

    private enum RandomIntOutputParameter
    {
        RandomInt
    }
}
