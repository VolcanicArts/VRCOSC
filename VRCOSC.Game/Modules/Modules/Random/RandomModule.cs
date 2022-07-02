// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics.Colour;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Random;

public abstract class RandomModule<T> : Module where T : struct
{
    public override string Title => $"Random {TypeUtils.TypeToReadableName<T>()}";
    public override string Description => $"Sends a random {TypeUtils.TypeToReadableName<T>().ToLower()} over a variable time period";
    public override string Author => "VolcanicArts";
    public override ColourInfo Colour => ColourInfo.GradientVertical(VRCOSCColour.Red, VRCOSCColour.PurpleDarker);
    public override ModuleType ModuleType => ModuleType.Random;
    protected override double DeltaUpdate => GetSetting<int>(RandomSetting.DeltaUpdate);

    public override void CreateAttributes()
    {
        CreateSetting(RandomSetting.DeltaUpdate, "Time Between Value", "The amount of time, in milliseconds, between each random value", 1000);

        var readableTypeName = TypeUtils.TypeToReadableName<T>();
        CreateOutputParameter(RandomOutputParameter.RandomValue, $"Random {readableTypeName}", $"A random {readableTypeName.ToLower()} value", $"/avatar/parameters/Random{readableTypeName}");
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
