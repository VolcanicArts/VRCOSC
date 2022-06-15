// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Modules.Modules.Clock;

public class ClockModule : Module
{
    public override string Title => "Clock";
    public override string Description => "Sends your local time as hours, minutes, and seconds";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => VRCOSCColour.Blue.Darken(0.25f);
    public override ModuleType Type => ModuleType.General;
    public override double DeltaUpdate => GetSetting<bool>(ClockSettings.SmoothSecond) ? 50d : 1000d;

    public override void CreateAttributes()
    {
        CreateSetting(ClockSettings.UTC, "UTC", "Send the time as UTC rather than your local time", false);
        CreateSetting(ClockSettings.SmoothSecond, "Smooth Second", "If the seconds hand should be smooth", false);

        CreateOutputParameter(ClockParameters.Hours, "Hour", "The current hour normalised", "/avatar/parameters/ClockHour");
        CreateOutputParameter(ClockParameters.Minutes, "Minute", "The current minute normalised", "/avatar/parameters/ClockMinute");
        CreateOutputParameter(ClockParameters.Seconds, "Second", "The current second normalised", "/avatar/parameters/ClockSecond");
    }

    public override void Update()
    {
        var sendAsUtc = GetSetting<bool>(ClockSettings.UTC);

        float hour, minute, second, millisecond;

        if (sendAsUtc)
        {
            hour = DateTime.UtcNow.Hour;
            minute = DateTime.UtcNow.Minute;
            second = DateTime.UtcNow.Second;
            millisecond = DateTime.UtcNow.Millisecond;
        }
        else
        {
            hour = DateTime.Now.Hour;
            minute = DateTime.Now.Minute;
            second = DateTime.Now.Second;
            millisecond = DateTime.Now.Millisecond;
        }

        // smooth hands
        if (GetSetting<bool>(ClockSettings.SmoothSecond)) second += millisecond / 1000f;
        minute += second / 60f;
        hour += minute / 60f;

        var hourNormalised = (hour % 12f) / 12f;
        var minuteNormalised = minute / 60f;
        var secondNormalised = second / 60f;

        SendParameter(ClockParameters.Hours, hourNormalised);
        SendParameter(ClockParameters.Minutes, minuteNormalised);
        SendParameter(ClockParameters.Seconds, secondNormalised);
    }
}

public enum ClockParameters
{
    Hours,
    Minutes,
    Seconds,
}

public enum ClockSettings
{
    UTC,
    SmoothSecond
}
