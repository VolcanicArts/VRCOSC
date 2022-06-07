// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
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
    public override double DeltaUpdate => 1000d;

    protected override Dictionary<Enum, (string, string, object)> Settings => new()
    {
        { ClockSettings.UTC, ("UTC", "Send the time as UTC rather than your local time", false) }
    };

    protected override Dictionary<Enum, (string, string, string)> OutputParameters => new()
    {
        { ClockParameters.Hours, ("Hour", "The current hour normalised", "/avatar/parameters/ClockHour") },
        { ClockParameters.Minutes, ("Minute", "The current minute normalised", "/avatar/parameters/ClockMinute") },
        { ClockParameters.Seconds, ("Second", "The current second normalised", "/avatar/parameters/ClockSecond") },
    };

    protected override void OnUpdate()
    {
        var sendAsUTC = GetSettingAs<bool>(ClockSettings.UTC);

        int hour, minute, second;

        if (sendAsUTC)
        {
            hour = DateTime.UtcNow.Hour;
            minute = DateTime.UtcNow.Minute;
            second = DateTime.UtcNow.Second;
        }
        else
        {
            hour = DateTime.Now.Hour;
            minute = DateTime.Now.Minute;
            second = DateTime.Now.Second;
        }

        Terminal.Log($"Time is currently: {hour}:{minute}:{second}");

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
    UTC
}
