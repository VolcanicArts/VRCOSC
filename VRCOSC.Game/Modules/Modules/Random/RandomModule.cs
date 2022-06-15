// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;

namespace VRCOSC.Game.Modules.Modules.Random;

public class RandomModule : Module
{
    public override string Title => "Random";
    public override string Description => "Sends a random float between 0 and 1 every second";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Colour4.Coral.Darken(0.5f);
    public override ModuleType ModuleType => ModuleType.General;
    public override double DeltaUpdate => 1000d;

    private readonly System.Random random = new();

    public override void CreateAttributes()
    {
        CreateOutputParameter(RandomParameter.RandomValue, "Random Value", "A random float value between 0 and 1", "/avatar/parameters/RandomValue");
    }

    public override void Update()
    {
        float randomFloat = (float)random.NextDouble();
        SendParameter(RandomParameter.RandomValue, randomFloat);
    }
}

public enum RandomParameter
{
    RandomValue
}
