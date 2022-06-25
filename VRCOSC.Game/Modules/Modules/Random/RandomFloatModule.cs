// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;

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
        CreateSetting(RandomFloatSetting.DeltaUpdate, "Time Between Update", "The amount of time, in milliseconds, between each random value", 1000);

        CreateOutputParameter(RandomFloatOutputParameter.RandomFloat, "Random Float", "A random float value", "/avatar/parameters/RandomFloat");
    }

    protected override void OnUpdate()
    {
        float randomFloat = (float)random.NextDouble();
        SendParameter(RandomFloatOutputParameter.RandomFloat, randomFloat);
    }

    private enum RandomFloatSetting
    {
        DeltaUpdate
    }

    private enum RandomFloatOutputParameter
    {
        RandomFloat
    }
}
