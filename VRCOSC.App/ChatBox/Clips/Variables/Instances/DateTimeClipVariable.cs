// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class DateTimeClipVariable : ClipVariable
{
    public DateTimeClipVariable()
    {
    }

    public DateTimeClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("datetime_format", "Date/Time Format", "How should the date/time be formatted?")]
    public string DateTimeFormat { get; set; } = "yyyy/MM/dd HH:mm:ss";

    [ClipVariableOption("timezone_id", "Time Zone ID", "What timezone should this date/time be converted to?\nLeave empty for your local timezone\nNote: Daylight savings is handled automatically")]
    public string TimeZoneID { get; set; } = string.Empty;

    public override bool IsDefault() => base.IsDefault() && DateTimeFormat == "yyyy/MM/dd HH:mm:ss" && TimeZoneID == string.Empty;

    public override DateTimeClipVariable Clone()
    {
        var clone = (DateTimeClipVariable)base.Clone();

        clone.DateTimeFormat = DateTimeFormat;
        clone.TimeZoneID = TimeZoneID;

        return clone;
    }

    protected override string Format(object value)
    {
        var dateTimeValue = (DateTimeOffset)value;

        try
        {
            DateTime convertedDateTime;

            if (string.IsNullOrEmpty(TimeZoneID))
            {
                convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeValue.UtcDateTime, TimeZoneInfo.Local);
            }
            else
            {
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneID);
                convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeValue.UtcDateTime, timeZoneInfo);
            }

            try
            {
                return convertedDateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return "INVALID FORMAT";
            }
        }
        catch (Exception)
        {
            return "INVALID TIMEZONE";
        }
    }
}