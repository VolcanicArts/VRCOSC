// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using VRCOSC.Game.Modules.SDK;
using VRCOSC.Game.Modules.SDK.Parameters;

namespace VRCOSC.Modules;

[ModuleTitle("Clock")]
[ModuleDescription("Sends a chosen timezone as hours, minutes, and seconds")]
[ModuleType(ModuleType.Generic)]
[ModulePrefab("VRCOSC-Watch", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-Watch.unitypackage")]
public sealed class ClockModule : Module
{
    private DateTime time;

    protected override void OnLoad()
    {
        CreateToggle(ClockSetting.SmoothSecond, "Smooth Second", "If the seconds value should be smoothed", false);
        CreateToggle(ClockSetting.SmoothMinute, "Smooth Minute", "If the minutes value should be smoothed", true);
        CreateToggle(ClockSetting.SmoothHour, "Smooth Hour", "If the hours value should be smoothed", true);
        CreateDropdown(ClockSetting.Timezone, "Timezone", "The timezone the clock should follow", ClockTimeZone.Local);

        RegisterParameter<float>(ClockParameter.Hours, "VRCOSC/Clock/Hours", ParameterMode.Write, "Hours", "The current hour normalised between 0 and 1");
        RegisterParameter<float>(ClockParameter.Minutes, "VRCOSC/Clock/Minutes", ParameterMode.Write, "Minutes", "The current minute normalised between 0 and 1");
        RegisterParameter<float>(ClockParameter.Seconds, "VRCOSC/Clock/Seconds", ParameterMode.Write, "Seconds", "The current second normalised between 0 and 1");
        RegisterParameter<bool>(ClockParameter.Period, "VRCOSC/Clock/Period", ParameterMode.Write, "Period", "False for AM. True for PM");

        CreateGroup("Smoothing", ClockSetting.SmoothHour, ClockSetting.SmoothMinute, ClockSetting.SmoothSecond);
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void updateVariables()
    {
        time = timezoneToTime(GetSettingValue<ClockTimeZone>(ClockSetting.Timezone));

        var hours = GetSettingValue<bool>(ClockSetting.SmoothHour) ? getSmoothedHours(time) : time.Hour;
        var minutes = GetSettingValue<bool>(ClockSetting.SmoothMinute) ? getSmoothedMinutes(time) : time.Minute;
        var seconds = GetSettingValue<bool>(ClockSetting.SmoothSecond) ? getSmoothedSeconds(time) : time.Second;

        var hourNormalised = hours % 12f / 12f;
        var minuteNormalised = minutes / 60f;
        var secondNormalised = seconds / 60f;

        SendParameter(ClockParameter.Hours, hourNormalised);
        SendParameter(ClockParameter.Minutes, minuteNormalised);
        SendParameter(ClockParameter.Seconds, secondNormalised);
        SendParameter(ClockParameter.Period, string.Equals(time.ToString("tt", CultureInfo.InvariantCulture), "PM", StringComparison.InvariantCultureIgnoreCase));
    }

    private static float getSmoothedSeconds(DateTime time) => time.Second + time.Millisecond / 1000f;
    private static float getSmoothedMinutes(DateTime time) => time.Minute + getSmoothedSeconds(time) / 60f;
    private static float getSmoothedHours(DateTime time) => time.Hour + getSmoothedMinutes(time) / 60f;

    private static DateTime timezoneToTime(ClockTimeZone timeZone) => timeZone switch
    {
        ClockTimeZone.Local => DateTime.Now,
        ClockTimeZone.UTC => DateTime.UtcNow,
        ClockTimeZone.GMT => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")),
        ClockTimeZone.EST => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
        ClockTimeZone.CST => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")),
        ClockTimeZone.MNT => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")),
        ClockTimeZone.PST => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")),
        _ => throw new ArgumentOutOfRangeException(nameof(timeZone), timeZone, null)
    };

    private enum ClockParameter
    {
        Hours,
        Minutes,
        Seconds,
        Period
    }

    private enum ClockSetting
    {
        Timezone,
        SmoothSecond,
        SmoothMinute,
        SmoothHour
    }

    private enum ClockTimeZone
    {
        Local,
        UTC,
        GMT,
        EST,
        CST,
        MNT,
        PST
    }
}
