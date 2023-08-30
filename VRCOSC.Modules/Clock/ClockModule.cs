// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;

namespace VRCOSC.Modules.Clock;

[ModuleTitle("Clock")]
[ModuleDescription("Sends a chosen time as hours, minutes, and seconds")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.General)]
[ModulePrefab("VRCOSC-Watch", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-Watch.unitypackage")]
public sealed class ClockModule : ChatBoxModule
{
    private DateTime time;

    protected override void CreateAttributes()
    {
        CreateSetting(ClockSetting.SmoothSecond, "Smooth Second", "If the seconds value should be smoothed", false);
        CreateSetting(ClockSetting.SmoothMinute, "Smooth Minute", "If the minutes value should be smoothed", true);
        CreateSetting(ClockSetting.SmoothHour, "Smooth Hour", "If the hours value should be smoothed", true);
        CreateSetting(ClockSetting.Mode, "Mode", "If the clock should be in 12 hour or 24 hour\nNote that this only affects the ChatBox", ClockMode.Twelve);
        CreateSetting(ClockSetting.Timezone, "Timezone", "The timezone the clock should follow", ClockTimeZone.Local);

        CreateParameter<float>(ClockParameter.Hours, ParameterMode.Write, "VRCOSC/Clock/Hours", "Hours", "The current hour normalised");
        CreateParameter<float>(ClockParameter.Minutes, ParameterMode.Write, "VRCOSC/Clock/Minutes", "Minutes", "The current minute normalised");
        CreateParameter<float>(ClockParameter.Seconds, ParameterMode.Write, "VRCOSC/Clock/Seconds", "Seconds", "The current second normalised");
        CreateParameter<bool>(ClockParameter.Period, ParameterMode.Write, "VRCOSC/Clock/Period", "Period", "False for AM, true for PM");

        CreateVariable(ClockVariable.Hours, "Hours", "h");
        CreateVariable(ClockVariable.Minutes, "Minutes", "m");
        CreateVariable(ClockVariable.Seconds, "Seconds", "s");
        CreateVariable(ClockVariable.Period, "AM/PM", "period");
        CreateVariable(ClockVariable.Symbol, "Symbol", "symbol");

        CreateState(ClockState.Default, "Default", $"Local Time/v{GetVariableFormat(ClockVariable.Hours)}:{GetVariableFormat(ClockVariable.Minutes)}{GetVariableFormat(ClockVariable.Period)}");
    }

    protected override void OnModuleStart()
    {
        ChangeStateTo(ClockState.Default);
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void updateVariables()
    {
        time = timezoneToTime(GetSetting<ClockTimeZone>(ClockSetting.Timezone));

        var hours = GetSetting<bool>(ClockSetting.SmoothHour) ? getSmoothedHours(time) : time.Hour;
        var minutes = GetSetting<bool>(ClockSetting.SmoothMinute) ? getSmoothedMinutes(time) : time.Minute;
        var seconds = GetSetting<bool>(ClockSetting.SmoothSecond) ? getSmoothedSeconds(time) : time.Second;

        var hourNormalised = hours % 12f / 12f;
        var minuteNormalised = minutes / 60f;
        var secondNormalised = seconds / 60f;

        SendParameter(ClockParameter.Hours, hourNormalised);
        SendParameter(ClockParameter.Minutes, minuteNormalised);
        SendParameter(ClockParameter.Seconds, secondNormalised);
        SendParameter(ClockParameter.Period, string.Equals(time.ToString("tt", CultureInfo.InvariantCulture), "PM", StringComparison.InvariantCultureIgnoreCase));

        string hourText, minuteText, secondText, periodText;

        if (GetSetting<ClockMode>(ClockSetting.Mode) == ClockMode.Twelve)
        {
            hourText = time.ToString("hh", CultureInfo.InvariantCulture);
            minuteText = time.ToString("mm", CultureInfo.InvariantCulture);
            secondText = time.ToString("ss", CultureInfo.InvariantCulture);
            periodText = time.ToString("tt", CultureInfo.InvariantCulture).ToLowerInvariant();
        }
        else
        {
            hourText = time.ToString("HH", CultureInfo.InvariantCulture);
            minuteText = time.ToString("mm", CultureInfo.InvariantCulture);
            secondText = time.ToString("ss", CultureInfo.InvariantCulture);
            periodText = string.Empty;
        }

        SetVariableValue(ClockVariable.Hours, hourText);
        SetVariableValue(ClockVariable.Minutes, minuteText);
        SetVariableValue(ClockVariable.Seconds, secondText);
        SetVariableValue(ClockVariable.Period, periodText);
        SetVariableValue(ClockVariable.Symbol, getClockSymbol(time));
    }

    private static string getClockSymbol(DateTime time)
    {
        var offset = getClockSymbolCharacter(time);

        var surrogate1 = 0xD800 + ((offset - 0x10000) >> 10);
        var surrogate2 = 0xDC00 + ((offset - 0x10000) & 0x3FF);

        var newCodePoint = ((surrogate1 - 0xD800) << 10) + (surrogate2 - 0xDC00) + 0x10000;

        return char.ConvertFromUtf32(newCodePoint);
    }

    private static int getClockSymbolCharacter(DateTime time)
    {
        const int base_code_point = 128336;
        const int half_past_base = 128348;

        var hour = (time.Hour + 11) % 12;
        return time.Minute >= 30 ? half_past_base + hour : base_code_point + hour;
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
        SmoothHour,
        Mode
    }

    private enum ClockState
    {
        Default
    }

    private enum ClockVariable
    {
        Hours,
        Minutes,
        Seconds,
        Period,
        Symbol
    }

    private enum ClockMode
    {
        Twelve,
        TwentyFour
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
