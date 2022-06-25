// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Random;

public class RandomFloatModule : Module
{
    public override string Title => "Random Float";
    public override string Description => "Sends a random float over a variable time period";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Colour4.Coral.Darken(0.5f);
    public override ModuleType ModuleType => ModuleType.General;
    protected override double DeltaUpdate => GetSetting<int>(RandomFloatSetting.DeltaUpdate);

    private readonly System.Random random = new();

    public override void CreateAttributes()
    {
        CreateSetting(RandomFloatSetting.DeltaUpdate, "Time Between Value", "The amount of time, in milliseconds, between each random value", 1000);
        CreateSetting(RandomFloatSetting.MinValue, "Min Value", "The minimum value of the float", 0f, 0f, 1f);
        CreateSetting(RandomFloatSetting.MaxValue, "Max Value", "The maximum value of the float", 1f, 0f, 1f);

        CreateOutputParameter(RandomFloatOutputParameter.RandomFloat, "Random Float", "A random float value", "/avatar/parameters/RandomFloat");
    }

    protected override void OnUpdate()
    {
        float randomFloat = (float)random.NextDouble();
        float randomFloatMapped = ModuleMaths.Map(randomFloat, 0, 1, GetSetting<float>(RandomFloatSetting.MinValue), GetSetting<float>(RandomFloatSetting.MaxValue));
        SendParameter(RandomFloatOutputParameter.RandomFloat, randomFloatMapped);
    }

    private enum RandomFloatSetting
    {
        DeltaUpdate,
        MinValue,
        MaxValue
    }

    private enum RandomFloatOutputParameter
    {
        RandomFloat
    }
}
