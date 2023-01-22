// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Modules.ChatBoxText;
using VRCOSC.Modules.Clock;
using VRCOSC.Modules.Discord;
using VRCOSC.Modules.HardwareStats;
using VRCOSC.Modules.Heartrate.HypeRate;
using VRCOSC.Modules.Heartrate.Pulsoid;
using VRCOSC.Modules.Media;
using VRCOSC.Modules.OpenVR;
using VRCOSC.Modules.Random;
using VRCOSC.Modules.Weather;

namespace VRCOSC.Modules;

public static class InternalModules
{
    public static readonly IReadOnlyList<Type> MODULE_TYPES = new[]
    {
        typeof(HypeRateModule),
        typeof(PulsoidModule),
        typeof(OpenVRStatisticsModule),
        typeof(OpenVRControllerStatisticsModule),
        typeof(GestureExtensionsModule),
        typeof(MediaModule),
        typeof(DiscordModule),
        typeof(ClockModule),
        typeof(ChatBoxTextModule),
        typeof(HardwareStatsModule),
        typeof(WeatherModule),
        typeof(RandomBoolModule),
        typeof(RandomIntModule),
        typeof(RandomFloatModule)
    };
}
