// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.Clock;

public sealed class ClockModule : ChatBoxModule
{
    public override string Title => "Clock";
    public override string Description => "Sends your local time as hours, minutes, and seconds";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Watch";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => GetSetting<bool>(ClockSetting.SmoothSecond) ? vrc_osc_delta_update : 1000;

    protected override bool DefaultChatBoxDisplay => true;
    protected override string DefaultChatBoxFormat => "Local Time                                %h%:%m%%period%";
    protected override IEnumerable<string> ChatBoxFormatValues => new[] { "%h%", "%m%", "%s%", "%period%" };

    private DateTime time;

    protected override void CreateAttributes()
    {
        CreateSetting(ClockSetting.SmoothSecond, "Smooth Second", "If the seconds value should be smoothed", false);
        CreateSetting(ClockSetting.SmoothMinute, "Smooth Minute", "If the minutes value should be smoothed", true);
        CreateSetting(ClockSetting.SmoothHour, "Smooth Hour", "If the hours value should be smoothed", true);
        CreateSetting(ClockSetting.Mode, "Mode", "If the clock should be in 12 hour or 24 hour", ClockMode.Twelve);
        CreateSetting(ClockSetting.Timezone, "Timezone", "The timezone the clock should follow", ClockTimeZone.Local);

        base.CreateAttributes();

        CreateParameter<float>(ClockParameter.Hours, ParameterMode.Write, "VRCOSC/Clock/Hours", "The current hour normalised");
        CreateParameter<float>(ClockParameter.Minutes, ParameterMode.Write, "VRCOSC/Clock/Minutes", "The current minute normalised");
        CreateParameter<float>(ClockParameter.Seconds, ParameterMode.Write, "VRCOSC/Clock/Seconds", "The current second normalised");
    }

    protected override string? GetChatBoxText()
    {
        var chatBoxTime = timezoneToTime(GetSetting<ClockTimeZone>(ClockSetting.Timezone));
        var textHour = GetSetting<ClockMode>(ClockSetting.Mode) == ClockMode.Twelve ? (chatBoxTime.Hour % 12).ToString("00") : chatBoxTime.Hour.ToString("00");
        return GetSetting<string>(ChatBoxSetting.ChatBoxFormat)
               .Replace("%h%", textHour)
               .Replace("%m%", chatBoxTime.Minute.ToString("00"))
               .Replace("%s%", chatBoxTime.Second.ToString("00"))
               .Replace("%period%", chatBoxTime.Hour >= 12 ? "pm" : "am");
    }

    protected override Task OnUpdate()
    {
        time = timezoneToTime(GetSetting<ClockTimeZone>(ClockSetting.Timezone));

        var hours = GetSetting<bool>(ClockSetting.SmoothHour) ? getSmoothedHours(time) : time.Hour;
        var minutes = GetSetting<bool>(ClockSetting.SmoothMinute) ? getSmoothedMinutes(time) : time.Minute;
        var seconds = GetSetting<bool>(ClockSetting.SmoothSecond) ? getSmoothedSeconds(time) : time.Second;

        var normalisationComponent = GetSetting<ClockMode>(ClockSetting.Mode) == ClockMode.Twelve ? 12f : 24f;
        var hourNormalised = (hours % normalisationComponent) / normalisationComponent;
        var minuteNormalised = minutes / 60f;
        var secondNormalised = seconds / 60f;

        SendParameter(ClockParameter.Hours, hourNormalised);
        SendParameter(ClockParameter.Minutes, minuteNormalised);
        SendParameter(ClockParameter.Seconds, secondNormalised);

        return Task.CompletedTask;
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
        Seconds,
    }

    private enum ClockSetting
    {
        Timezone,
        SmoothSecond,
        SmoothMinute,
        SmoothHour,
        Mode
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
