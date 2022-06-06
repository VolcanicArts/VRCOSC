// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;

namespace VRCOSC.Game.Modules.Modules.Clock;

public class ClockModule : Module
{
    public override string Title => "Clock";
    public override string Description => "Sends your current local time in 2 different formats";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Colour4.LightBlue.Darken(0.5f);
    public override ModuleType Type => ModuleType.General;
    public override double DeltaUpdate => 1000d;

    protected override Dictionary<Enum, (string, string, object)> Settings => new()
    {
        { ClockSetting.SendType, ("Send Type", "Whether to send the clock data for blendtree (-1,1) or as individual digits", ClockSettingSendType.SendAsIndividual) }
    };

    protected override Dictionary<Enum, (string, string, string)> OutputParameters => new()
    {
        { ClockParameter.HoursTens, ("Hours Tens", "The tens value of the current hour", "/avatar/parameters/HoursTens") },
        { ClockParameter.HoursUnits, ("Hours Units", "The units value of the current hour", "/avatar/parameters/HoursUnits") },
        { ClockParameter.MinutesTens, ("Minutes Tens", "The tens value of the current minute", "/avatar/parameters/MinutesTens") },
        { ClockParameter.MinutesUnits, ("Minutes Units", "The units value of the current minute", "/avatar/parameters/MinutesUnits") },
        { ClockParameter.HourMapped, ("Hour Mapped", "The current hour mapped between -1 and 1", "/avatar/parameters/HourMapped") },
        { ClockParameter.MinuteMapped, ("Minute Mapped", "The current minute mapped between -1 and 1", "/avatar/parameters/MinuteMapped") }
    };

    protected override void OnUpdate()
    {
        var sendType = GetSettingAs<ClockSettingSendType>(ClockSetting.SendType);

        switch (sendType)
        {
            case ClockSettingSendType.SendForBlendtree:
                sendForBlendTree();
                break;

            case ClockSettingSendType.SendAsIndividual:
                sendAsIndividual();
                break;
        }
    }

    private void sendForBlendTree()
    {
        var hour = DateTime.Now.Hour;
        var minute = DateTime.Now.Minute;

        var hourMapped = MathF.Round(MapBetween(hour, 0, 24, -1, 1), 2);
        var minuteMapped = MathF.Round(MapBetween(minute, 0, 60, -1, 1), 2);

        Terminal.Log("Hour mapped " + hourMapped);
        Terminal.Log("Minute mapped " + minuteMapped);

        SendParameter(ClockParameter.HourMapped, hourMapped);
        SendParameter(ClockParameter.MinuteMapped, minuteMapped);
    }

    private void sendAsIndividual()
    {
        var hour = DateTime.Now.Hour;
        var minute = DateTime.Now.Minute;

        var hoursSplit = ToDigitArray(hour, 2);
        var minutesSplit = ToDigitArray(minute, 2);

        Terminal.Log($"Time is currently: {hoursSplit[0]}{hoursSplit[1]}:{minutesSplit[0]}{minutesSplit[1]}");

        SendParameter(ClockParameter.HoursTens, hoursSplit[0]);
        SendParameter(ClockParameter.HoursUnits, hoursSplit[1]);
        SendParameter(ClockParameter.MinutesTens, minutesSplit[0]);
        SendParameter(ClockParameter.MinutesUnits, minutesSplit[1]);
    }
}

public enum ClockParameter
{
    HoursUnits,
    HoursTens,
    MinutesUnits,
    MinutesTens,
    HourMapped,
    MinuteMapped
}

public enum ClockSetting
{
    SendType
}

public enum ClockSettingSendType
{
    SendForBlendtree,
    SendAsIndividual
}
