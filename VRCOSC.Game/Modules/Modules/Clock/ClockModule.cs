// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.Modules.Clock;

public sealed class ClockModule : Module
{
    public override string Title => "Clock";
    public override string Description => "Sends your local time as hours, minutes, and seconds";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Watch";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => GetSetting<bool>(ClockSetting.SmoothSecond) ? vrc_osc_delta_update : 1000;

    protected override void CreateAttributes()
    {
        CreateSetting(ClockSetting.UseChatBox, "Use ChatBox", "Should the module display the time in the ChatBox?", false);
        CreateSetting(ClockSetting.ChatBoxFormat, "ChatBox Format", "The format for displaying the time in the ChatBox.\nAvailable values: %h%, %m%, %s%, %period%", "%h%:%m%%period%");
        CreateSetting(ClockSetting.SmoothSecond, "Smooth Second", "If the seconds value should be smoothed", false);
        CreateSetting(ClockSetting.SmoothMinute, "Smooth Minute", "If the minutes value should be smoothed", true);
        CreateSetting(ClockSetting.SmoothHour, "Smooth Hour", "If the hours value should be smoothed", true);
        CreateSetting(ClockSetting.Mode, "Mode", "If the clock should be in 12 hour or 24 hour", ClockMode.Twelve);
        CreateSetting(ClockSetting.Timezone, "Timezone", "The timezone the clock should follow", ClockTimeZone.Local);

        CreateOutgoingParameter(ClockOutgoingParameter.Hours, "Hour", "The current hour normalised", "/avatar/parameters/VRCOSC/Clock/Hours");
        CreateOutgoingParameter(ClockOutgoingParameter.Minutes, "Minute", "The current minute normalised", "/avatar/parameters/VRCOSC/Clock/Minutes");
        CreateOutgoingParameter(ClockOutgoingParameter.Seconds, "Second", "The current second normalised", "/avatar/parameters/VRCOSC/Clock/Seconds");
    }

    protected override void OnUpdate()
    {
        var time = timezoneToTime(GetSetting<ClockTimeZone>(ClockSetting.Timezone));

        var hours = GetSetting<bool>(ClockSetting.SmoothHour) ? getSmoothedHours(time) : time.Hour;
        var minutes = GetSetting<bool>(ClockSetting.SmoothMinute) ? getSmoothedMinutes(time) : time.Minute;
        var seconds = GetSetting<bool>(ClockSetting.SmoothSecond) ? getSmoothedSeconds(time) : time.Second;

        var normalisationComponent = GetSetting<ClockMode>(ClockSetting.Mode) == ClockMode.Twelve ? 12f : 24f;
        var hourNormalised = (hours % normalisationComponent) / normalisationComponent;
        var minuteNormalised = minutes / 60f;
        var secondNormalised = seconds / 60f;

        SendParameter(ClockOutgoingParameter.Hours, hourNormalised);
        SendParameter(ClockOutgoingParameter.Minutes, minuteNormalised);
        SendParameter(ClockOutgoingParameter.Seconds, secondNormalised);

        if (GetSetting<bool>(ClockSetting.UseChatBox))
        {
            var textHour = GetSetting<ClockMode>(ClockSetting.Mode) == ClockMode.Twelve ? (time.Hour % 12).ToString("00") : time.Hour.ToString("00");

            var text = GetSetting<string>(ClockSetting.ChatBoxFormat)
                       .Replace("%h%", textHour)
                       .Replace("%m%", time.Minute.ToString("00"))
                       .Replace("%s%", time.Second.ToString("00"))
                       .Replace("%period%", time.Hour >= 12 ? "pm" : "am");

            SetChatBoxText(text);
        }
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

    private enum ClockOutgoingParameter
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
        Mode,
        UseChatBox,
        ChatBoxFormat
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
