// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Graphics.Colour;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Modules.Modules.Clock;

public class ClockModule : Module
{
    public override string Title => "Clock";
    public override string Description => "Sends your local time as hours, minutes, and seconds";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Watch";
    public override ColourInfo Colour => VRCOSCColour.Blue;
    public override ModuleType ModuleType => ModuleType.General;
    protected override double DeltaUpdate => GetSetting<bool>(ClockSetting.SmoothSecond) ? 50d : 1000d;

    protected override void CreateAttributes()
    {
        CreateSetting(ClockSetting.Timezone, "Timezone", "The timezone the watch should follow", ClockTimeZone.Local);
        CreateSetting(ClockSetting.SmoothSecond, "Smooth Second", "If the seconds hand should be smooth", false);

        CreateOutputParameter(ClockOutputParameter.Hours, "Hour", "The current hour normalised", "/avatar/parameters/ClockHour");
        CreateOutputParameter(ClockOutputParameter.Minutes, "Minute", "The current minute normalised", "/avatar/parameters/ClockMinute");
        CreateOutputParameter(ClockOutputParameter.Seconds, "Second", "The current second normalised", "/avatar/parameters/ClockSecond");
    }

    protected override void OnUpdate()
    {
        var time = timezoneToTime(GetSetting<ClockTimeZone>(ClockSetting.Timezone));

        // smooth hands
        if (GetSetting<bool>(ClockSetting.SmoothSecond)) time.Second += time.Millisecond / 1000f;
        time.Minute += time.Second / 60f;
        time.Hour += time.Minute / 60f;

        var hourNormalised = (time.Hour % 12f) / 12f;
        var minuteNormalised = time.Minute / 60f;
        var secondNormalised = time.Second / 60f;

        SendParameter(ClockOutputParameter.Hours, hourNormalised);
        SendParameter(ClockOutputParameter.Minutes, minuteNormalised);
        SendParameter(ClockOutputParameter.Seconds, secondNormalised);
    }

    private static Time timezoneToTime(ClockTimeZone timeZone)
    {
        return timeZone switch
        {
            ClockTimeZone.Local => new Time { Hour = DateTime.Now.Hour, Minute = DateTime.Now.Minute, Second = DateTime.Now.Second, Millisecond = DateTime.Now.Millisecond },
            ClockTimeZone.UTC => new Time { Hour = DateTime.UtcNow.Hour, Minute = DateTime.UtcNow.Minute, Second = DateTime.UtcNow.Second, Millisecond = DateTime.UtcNow.Millisecond },
            _ => throw new ArgumentOutOfRangeException(nameof(timeZone), timeZone, null)
        };
    }

    private struct Time
    {
        public float Hour;
        public float Minute;
        public float Second;
        public float Millisecond;
    }

    private enum ClockOutputParameter
    {
        Hours,
        Minutes,
        Seconds,
    }

    private enum ClockSetting
    {
        Timezone,
        SmoothSecond
    }

    private enum ClockTimeZone
    {
        Local,
        UTC
    }
}
