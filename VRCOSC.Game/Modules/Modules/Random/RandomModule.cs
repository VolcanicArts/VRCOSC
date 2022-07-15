// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics.Colour;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Random;

public abstract class RandomModule<T> : Module where T : struct
{
    public override string Title => $"Random {typeof(T).ToReadableName()}";
    public override string Description => $"Sends a random {typeof(T).ToReadableName().ToLowerInvariant()} over a variable time period";
    public override string Author => "VolcanicArts";
    public override ColourInfo Colour => ColourInfo.GradientVertical(VRCOSCColour.Red, VRCOSCColour.PurpleDarker);
    public override ModuleType ModuleType => ModuleType.Random;
    protected override double DeltaUpdate => GetSetting<int>(RandomSetting.DeltaUpdate);

    protected override void CreateAttributes()
    {
        CreateSetting(RandomSetting.DeltaUpdate, "Time Between Value", "The amount of time, in milliseconds, between each random value", 1000);

        var readableTypeName = typeof(T).ToReadableName();
        CreateOutputParameter(RandomOutputParameter.RandomValue, $"Random {readableTypeName}", $"A random {readableTypeName.ToLowerInvariant()} value", $"/avatar/parameters/Random{readableTypeName}");
    }

    protected override void OnUpdate()
    {
        SendParameter(RandomOutputParameter.RandomValue, GetRandomValue());
    }

    protected abstract T GetRandomValue();

    private enum RandomSetting
    {
        DeltaUpdate
    }

    private enum RandomOutputParameter
    {
        RandomValue
    }
}
