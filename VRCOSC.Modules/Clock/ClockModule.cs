// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.ChatBox;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Modules.Clock;

public sealed class ClockModule : ChatBoxModule
{
    public override string Title => "Clock";
    public override string Description => "Sends your local time as hours, minutes, and seconds";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Watch";
    public override ModuleType Type => ModuleType.General;
    protected override TimeSpan DeltaUpdate => GetSetting<bool>(ClockSetting.SmoothSecond) ? VRChatOscConstants.UPDATE_TIME_SPAN : TimeSpan.FromSeconds(1);

    private DateTime time;

    protected override void CreateAttributes()
    {
        CreateSetting(ClockSetting.SmoothSecond, "Smooth Second", "If the seconds value should be smoothed", false);
        CreateSetting(ClockSetting.SmoothMinute, "Smooth Minute", "If the minutes value should be smoothed", true);
        CreateSetting(ClockSetting.SmoothHour, "Smooth Hour", "If the hours value should be smoothed", true);
        CreateSetting(ClockSetting.Mode, "Mode", "If the clock should be in 12 hour or 24 hour", ClockMode.Twelve);
        CreateSetting(ClockSetting.Timezone, "Timezone", "The timezone the clock should follow", ClockTimeZone.Local);

        CreateParameter<float>(ClockParameter.Hours, ParameterMode.Write, "VRCOSC/Clock/Hours", "Hours", "The current hour normalised");
        CreateParameter<float>(ClockParameter.Minutes, ParameterMode.Write, "VRCOSC/Clock/Minutes", "Minutes", "The current minute normalised");
        CreateParameter<float>(ClockParameter.Seconds, ParameterMode.Write, "VRCOSC/Clock/Seconds", "Seconds", "The current second normalised");

        CreateVariable(ClockVariable.Hours, "Hours", "h");
        CreateVariable(ClockVariable.Minutes, "Minutes", "m");
        CreateVariable(ClockVariable.Seconds, "Seconds", "s");
        CreateVariable(ClockVariable.Period, "AM/PM", "period");

        CreateState(ClockState.Default, "Default", $"Local Time/n{GetVariableFormat(ClockVariable.Hours)}:{GetVariableFormat(ClockVariable.Minutes)}{GetVariableFormat(ClockVariable.Period)}");
    }

    protected override void OnModuleStart()
    {
        ChangeStateTo(ClockState.Default);
    }

    protected override void OnModuleUpdate()
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

        string hourText, minuteText, secondText, periodText;

        if (GetSetting<ClockMode>(ClockSetting.Mode) == ClockMode.Twelve)
        {
            var formattedTime = time.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture);
            var timeSplitPeriod = formattedTime.Split(new[] { ' ' }, 2);
            var timeText = timeSplitPeriod[0];

            var timeTextSplit = timeText.Split(new[] { ':' }, 3);

            hourText = timeTextSplit[0];
            minuteText = timeTextSplit[1];
            secondText = timeTextSplit[2];
            periodText = timeSplitPeriod[1].ToLowerInvariant();
        }
        else
        {
            var formattedTime = time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var timeTextSplit = formattedTime.Split(new[] { ':' }, 3);

            hourText = timeTextSplit[0];
            minuteText = timeTextSplit[1];
            secondText = timeTextSplit[2];
            periodText = string.Empty;
        }

        SetVariableValue(ClockVariable.Hours, hourText);
        SetVariableValue(ClockVariable.Minutes, minuteText);
        SetVariableValue(ClockVariable.Seconds, secondText);
        SetVariableValue(ClockVariable.Period, periodText);
    }

    private static float getSmoothedSeconds(DateTime time) => time.Second + time.Millisecond / 1000f;
    private static float getSmoothedMinutes(DateTime time) => time.Minute + getSmoothedSeconds(time) / 60f;
    private static float getSmoothedHours(DateTime time) => time.Hour + getSmoothedMinutes(time) / 60f;

    private static DateTime timezoneToTime(ClockTimeZone timeZone)
    {
        return timeZone switch
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
    }

    private enum ClockParameter
    {
        Hours,
        Minutes,
        Seconds
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
        Period
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
