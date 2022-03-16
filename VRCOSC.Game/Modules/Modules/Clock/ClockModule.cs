// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Platform;

namespace VRCOSC.Game.Modules.Modules.Clock;

public class ClockModule : Module
{
    public override string Title => "Clock";
    public override string Description => "Sends hours and minutes in individual digits to VRChat";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Colour4.LightGray;
    public override ModuleType Type => ModuleType.General;
    public override double DeltaUpdate => 1000d;

    public ClockModule(Storage storage)
        : base(storage)
    {
        CreateParameter(ClockParameter.HoursTens, "Hours Tens", "The tens value of the current hour", "/avatar/parameters/HoursTens");
        CreateParameter(ClockParameter.HoursUnits, "Hours Units", "The units value of the current hour", "/avatar/parameters/HoursUnits");
        CreateParameter(ClockParameter.MinutesTens, "Minutes Tens", "The tens value of the current minute", "/avatar/parameters/MinutesTens");
        CreateParameter(ClockParameter.MinutesUnits, "Minutes Units", "The units value of the current minute", "/avatar/parameters/MinutesUnits");
    }

    protected override void OnUpdate()
    {
        var hour = DateTime.Now.Hour;
        var minute = DateTime.Now.Minute;

        var hoursSplit = toDigitArray(hour);
        var minutesSplit = toDigitArray(minute);

        Terminal.Log($"Time is currently: {hoursSplit[0]}{hoursSplit[1]}:{minutesSplit[0]}{minutesSplit[1]}");

        SendParameter(ClockParameter.HoursTens, hoursSplit[0]);
        SendParameter(ClockParameter.HoursUnits, hoursSplit[1]);
        SendParameter(ClockParameter.MinutesTens, minutesSplit[0]);
        SendParameter(ClockParameter.MinutesUnits, minutesSplit[1]);
    }

    private static int[] toDigitArray(int num)
    {
        var numStr = num.ToString().PadLeft(2, '0');
        return numStr.Select(digit => int.Parse(digit.ToString())).ToArray();
    }
}

public enum ClockParameter
{
    HoursUnits,
    HoursTens,
    MinutesUnits,
    MinutesTens
}
